using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Models;
using SMS.Models.Api;
using System.ComponentModel.DataAnnotations;

namespace SMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GradesController : ControllerBase
    {
        private readonly CimContext _context;

        public GradesController(CimContext context)
        {
            _context = context;
        }

        // GET: api/Grades/teacher/{teacherId}/courses
        [HttpGet("teacher/{teacherId}/courses")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetTeacherCourses(int teacherId)
        {
            try
            {
                var teacher = await _context.SMS_Teachers.FindAsync(teacherId);
                if (teacher == null)
                {
                    return NotFound(ApiResponse<List<object>>.Error("教师不存在"));
                }

                var courses = await _context.SMS_Courses
                    .Where(c => c.TeacherId == teacherId)
                    .Select(c => new
                    {
                        c.Id,
                        c.CID,
                        c.Name,
                        c.Credits,
                        c.CourseStatus,
                        EnrollmentCount = _context.SMS_Enrollments.Count(e => e.CourseCID == c.CID && e.Status == "Active")
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<object>>.Success(courses.Cast<object>().ToList(), "获取教师课程成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.Error($"获取教师课程失败: {ex.Message}"));
            }
        }

        // GET: api/Grades/course/{courseId}/enrollments
        [HttpGet("course/{courseId}/enrollments")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetCourseEnrollments(int courseId)
        {
            try
            {
                var course = await _context.SMS_Courses.FindAsync(courseId);
                if (course == null)
                {
                    return NotFound(ApiResponse<List<object>>.Error("课程不存在"));
                }

                var enrollments = await _context.SMS_Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .Where(e => e.CourseCID == course.CID && e.Status == "Active")
                    .Select(e => new
                    {
                        e.Id,
                        e.EID,
                        e.Grade,
                        e.Status,
                        e.Semester,
                        Student = new
                        {
                            e.Student!.Id,
                            e.Student.SID,
                            e.Student.Name,
                            e.Student.Major
                        },
                        Course = new
                        {
                            e.Course!.Id,
                            e.Course.CID,
                            e.Course.Name,
                            e.Course.Credits
                        }
                    })
                    .OrderBy(e => e.Student.SID)
                    .ToListAsync();

                return Ok(ApiResponse<List<object>>.Success(enrollments.Cast<object>().ToList(), "获取课程选课记录成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.Error($"获取课程选课记录失败: {ex.Message}"));
            }
        }

        // PUT: api/Grades/enrollment/{enrollmentId}/grade
        [HttpPut("enrollment/{enrollmentId}/grade")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateGrade(int enrollmentId, [FromBody] GradeUpdateModel request)
        {
            try
            {
                var enrollment = await _context.SMS_Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .FirstOrDefaultAsync(e => e.Id == enrollmentId);

                if (enrollment == null)
                {
                    return NotFound(ApiResponse<object>.Error("选课记录不存在"));
                }

                // 验证成绩范围
                if (request.Grade < 0 || request.Grade > 100)
                {
                    return BadRequest(ApiResponse<object>.Error("成绩必须在0-100之间"));
                }

                enrollment.Grade = request.Grade;
                enrollment.UpdateDate = DateTime.UtcNow;
                enrollment.UpdateUser = request.UpdateUser ?? "System";

                await _context.SaveChangesAsync();

                var result = new
                {
                    enrollment.Id,
                    enrollment.Grade,
                    Student = enrollment.Student?.Name,
                    Course = enrollment.Course?.Name,
                    UpdateTime = enrollment.UpdateDate
                };

                return Ok(ApiResponse<object>.Success(result, "成绩更新成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"更新成绩失败: {ex.Message}"));
            }
        }

        // POST: api/Grades/batch-update
        [HttpPost("batch-update")]
        public async Task<ActionResult<ApiResponse<object>>> BatchUpdateGrades([FromBody] BatchGradeUpdateModel request)
        {
            try
            {
                if (request.GradeUpdates == null || !request.GradeUpdates.Any())
                {
                    return BadRequest(ApiResponse<object>.Error("请提供要更新的成绩数据"));
                }

                var enrollmentIds = request.GradeUpdates.Select(g => g.EnrollmentId).ToList();
                var enrollments = await _context.SMS_Enrollments
                    .Where(e => enrollmentIds.Contains(e.Id))
                    .ToListAsync();

                var updateCount = 0;
                var errors = new List<string>();

                foreach (var gradeUpdate in request.GradeUpdates)
                {
                    var enrollment = enrollments.FirstOrDefault(e => e.Id == gradeUpdate.EnrollmentId);
                    if (enrollment == null)
                    {
                        errors.Add($"选课记录ID {gradeUpdate.EnrollmentId} 不存在");
                        continue;
                    }

                    if (gradeUpdate.Grade < 0 || gradeUpdate.Grade > 100)
                    {
                        errors.Add($"选课记录ID {gradeUpdate.EnrollmentId} 的成绩必须在0-100之间");
                        continue;
                    }

                    enrollment.Grade = gradeUpdate.Grade;
                    enrollment.UpdateDate = DateTime.UtcNow;
                    enrollment.UpdateUser = request.UpdateUser ?? "System";
                    updateCount++;
                }

                await _context.SaveChangesAsync();

                var result = new
                {
                    UpdatedCount = updateCount,
                    TotalCount = request.GradeUpdates.Count(),
                    Errors = errors
                };

                var message = errors.Any() ? 
                    $"批量更新完成，成功 {updateCount} 个，失败 {errors.Count} 个" : 
                    $"批量更新成功，共更新 {updateCount} 个成绩";

                return Ok(ApiResponse<object>.Success(result, message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"批量更新成绩失败: {ex.Message}"));
            }
        }

        // GET: api/Grades/student/{studentId}/report
        [HttpGet("student/{studentId}/report")]
        public async Task<ActionResult<ApiResponse<object>>> GetStudentGradeReport(int studentId)
        {
            try
            {
                var student = await _context.SMS_Students.FindAsync(studentId);
                if (student == null)
                {
                    return NotFound(ApiResponse<object>.Error("学生不存在"));
                }

                var enrollments = await _context.SMS_Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.StudentSID == student.SID)
                    .ToListAsync();

                var gradeReport = new
                {
                    Student = new
                    {
                        student.Id,
                        student.SID,
                        student.Name,
                        student.Major
                    },
                    TotalCourses = enrollments.Count,
                    CompletedCourses = enrollments.Count(e => e.Grade.HasValue),
                    TotalCredits = enrollments.Sum(e => e.Course?.Credits ?? 0),
                    EarnedCredits = enrollments.Where(e => e.Grade >= 60).Sum(e => e.Course?.Credits ?? 0),
                    GPA = enrollments.Where(e => e.Grade.HasValue).Any() ?
                        Math.Round(enrollments.Where(e => e.Grade.HasValue).Average(e => (double)e.Grade!.Value) / 10, 2) : 0.0,
                    Grades = enrollments.Select(e => new
                    {
                        e.Id,
                        Course = new
                        {
                            e.Course?.CID,
                            e.Course?.Name,
                            e.Course?.Credits
                        },
                        e.Grade,
                        e.Semester,
                        e.Status,
                        GradePoint = e.Grade.HasValue ? Math.Round((double)e.Grade.Value / 10, 1) : (double?)null
                    }).OrderByDescending(g => g.Semester).ToList()
                };

                return Ok(ApiResponse<object>.Success(gradeReport, "获取学生成绩报告成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"获取学生成绩报告失败: {ex.Message}"));
            }
        }

        // GET: api/Grades/statistics/course/{courseId}
        [HttpGet("statistics/course/{courseId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetCourseGradeStatistics(int courseId)
        {
            try
            {
                var course = await _context.SMS_Courses.FindAsync(courseId);
                if (course == null)
                {
                    return NotFound(ApiResponse<object>.Error("课程不存在"));
                }

                var enrollments = await _context.SMS_Enrollments
                    .Where(e => e.CourseCID == course.CID && e.Grade.HasValue)
                    .ToListAsync();

                if (!enrollments.Any())
                {
                    return Ok(ApiResponse<object>.Success(new { Message = "暂无成绩数据" }, "获取课程成绩统计成功"));
                }

                var grades = enrollments.Select(e => (double)e.Grade!.Value).ToList();
                var statistics = new
                {
                    Course = new
                    {
                        course.CID,
                        course.Name,
                        course.Credits
                    },
                    TotalStudents = enrollments.Count,
                    AverageGrade = Math.Round(grades.Average(), 2),
                    MaxGrade = grades.Max(),
                    MinGrade = grades.Min(),
                    PassRate = Math.Round((double)grades.Count(g => g >= 60) / grades.Count * 100, 2),
                    ExcellentRate = Math.Round((double)grades.Count(g => g >= 85) / grades.Count * 100, 2),
                    GradeDistribution = new
                    {
                        Excellent = grades.Count(g => g >= 85), // 优秀 85-100
                        Good = grades.Count(g => g >= 75 && g < 85), // 良好 75-84
                        Average = grades.Count(g => g >= 60 && g < 75), // 中等 60-74
                        Poor = grades.Count(g => g < 60) // 不及格 0-59
                    }
                };

                return Ok(ApiResponse<object>.Success(statistics, "获取课程成绩统计成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"获取课程成绩统计失败: {ex.Message}"));
            }
        }
    }

    // 成绩更新请求模型 - 专用于GradesController
    public class GradeUpdateModel
    {
        [Range(0, 100, ErrorMessage = "成绩必须在0-100之间")]
        public decimal Grade { get; set; }
        
        public string? UpdateUser { get; set; }
    }

    // 批量成绩更新请求模型
    public class BatchGradeUpdateModel
    {
        public List<SingleGradeUpdateModel> GradeUpdates { get; set; } = new List<SingleGradeUpdateModel>();
        public string? UpdateUser { get; set; }
    }

    public class SingleGradeUpdateModel
    {
        public int EnrollmentId { get; set; }
        
        [Range(0, 100, ErrorMessage = "成绩必须在0-100之间")]
        public decimal Grade { get; set; }
    }
}
