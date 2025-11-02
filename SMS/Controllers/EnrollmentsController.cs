using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Models;
using SMS.Models.Api;

namespace SMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly CimContext _context;

    public EnrollmentsController(CimContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有选课记录（分页）
    /// </summary>
    /// <param name="request">分页请求参数</param>
    /// <returns>选课记录分页列表</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<object>>>> GetEnrollments([FromQuery] PagedRequest request)
    {
        try
        {
            var query = _context.SMS_Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            
            var enrollments = await query
                .OrderByDescending(e => e.CreateDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new
                {
                    e.Id,
                    e.EID,
                    e.Status,
                    e.Semester,
                    e.Grade,
                    e.CreateDate,
                    Student = e.Student != null ? new
                    {
                        e.Student.Id,
                        e.Student.SID,
                        e.Student.Name,
                        e.Student.Major
                    } : null,
                    Course = e.Course != null ? new
                    {
                        e.Course.Id,
                        e.Course.CID,
                        e.Course.Name,
                        e.Course.Credits,
                        TeacherName = e.Course.Teacher != null ? e.Course.Teacher.Name : null
                    } : null
                })
                .ToListAsync();

            var result = new PagedResult<object>
            {
                Items = enrollments.Cast<object>().ToList(),
                TotalCount = totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };

            return Ok(ApiResponse<PagedResult<object>>.Success(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<object>>.Error($"获取选课记录失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 高级搜索选课记录
    /// </summary>
    /// <param name="request">搜索请求参数</param>
    /// <returns>搜索结果</returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<object>>>> SearchEnrollments([FromBody] EnrollmentSearchRequest request)
    {
        try
        {
            var query = _context.SMS_Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .AsQueryable();

            // 按学生学号搜索
            if (request.StudentSID.HasValue)
            {
                query = query.Where(e => e.StudentSID == request.StudentSID.Value);
            }

            // 按学生姓名搜索
            if (!string.IsNullOrEmpty(request.StudentName))
            {
                query = query.Where(e => e.Student != null && e.Student.Name != null && e.Student.Name.Contains(request.StudentName!));
            }

            // 按课程编号搜索
            if (request.CourseCID.HasValue)
            {
                query = query.Where(e => e.CourseCID == request.CourseCID.Value);
            }

            // 按课程名称搜索
            if (!string.IsNullOrEmpty(request.CourseName))
            {
                query = query.Where(e => e.Course != null && e.Course.Name != null && e.Course.Name.Contains(request.CourseName!));
            }

            // 按状态搜索
            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(e => e.Status == request.Status);
            }

            // 按学期搜索
            if (!string.IsNullOrEmpty(request.Semester))
            {
                query = query.Where(e => e.Semester == request.Semester);
            }

            // 按成绩范围搜索
            if (request.MinGrade.HasValue)
            {
                query = query.Where(e => e.Grade >= (decimal)request.MinGrade.Value);
            }
            if (request.MaxGrade.HasValue)
            {
                query = query.Where(e => e.Grade <= (decimal)request.MaxGrade.Value);
            }

            var totalCount = await query.CountAsync();
            
            var enrollments = await query
                .OrderByDescending(e => e.CreateDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new
                {
                    e.Id,
                    e.EID,
                    e.Status,
                    e.Semester,
                    e.Grade,
                    e.CreateDate,
                    Student = e.Student != null ? new
                    {
                        e.Student.Id,
                        e.Student.SID,
                        e.Student.Name,
                        e.Student.Major,
                        e.Student.Email
                    } : null,
                    Course = e.Course != null ? new
                    {
                        e.Course.Id,
                        e.Course.CID,
                        e.Course.Name,
                        e.Course.Credits,
                        TeacherName = e.Course.Teacher != null ? e.Course.Teacher.Name : null,
                        e.Course.Description
                    } : null
                })
                .ToListAsync();

            var result = new PagedResult<object>
            {
                Items = enrollments.Cast<object>().ToList(),
                TotalCount = totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };

            return Ok(ApiResponse<PagedResult<object>>.Success(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<object>>.Error($"搜索选课记录失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 根据ID获取特定选课记录
    /// </summary>
    /// <param name="id">选课记录ID</param>
    /// <returns>选课记录信息</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> GetEnrollment(int id)
    {
        try
        {
            var enrollment = await _context.SMS_Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.EID,
                    e.StudentSID,
                    e.CourseCID,
                    e.Status,
                    e.Semester,
                    e.Grade,
                    e.CreateDate,
                    e.UpdateDate,
                    e.CreateUser,
                    e.UpdateUser,
                    Student = e.Student != null ? new
                    {
                        e.Student.Id,
                        e.Student.SID,
                        e.Student.Name,
                        e.Student.Email,
                        e.Student.Major
                    } : null,
                    Course = e.Course != null ? new
                    {
                        e.Course.Id,
                        e.Course.CID,
                        e.Course.Name,
                        e.Course.Credits,
                        e.Course.Description,
                        TeacherName = e.Course.Teacher != null ? e.Course.Teacher.Name : null
                    } : null
                })
                .FirstOrDefaultAsync();

            if (enrollment == null)
            {
                return NotFound(ApiResponse<object>.Error("选课记录不存在"));
            }

            return Ok(ApiResponse<object>.Success(enrollment));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error($"获取选课记录失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 学生选课
    /// </summary>
    /// <param name="enrollment">选课信息</param>
    /// <returns>创建的选课记录</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Enrollment>>> CreateEnrollment(Enrollment enrollment)
    {
        try
        {
            // 检查学生是否存在
            var student = await _context.SMS_Students.FirstOrDefaultAsync(s => s.SID == enrollment.StudentSID);
            if (student == null)
            {
                return BadRequest(ApiResponse<Enrollment>.Error("学生不存在"));
            }

            // 检查课程是否存在
            var course = await _context.SMS_Courses.FirstOrDefaultAsync(c => c.CID == enrollment.CourseCID);
            if (course == null)
            {
                return BadRequest(ApiResponse<Enrollment>.Error("课程不存在"));
            }

            // 检查是否已经选过该课程
            var existingEnrollment = await _context.SMS_Enrollments
                .FirstOrDefaultAsync(e => e.StudentSID == enrollment.StudentSID && e.CourseCID == enrollment.CourseCID);
            if (existingEnrollment != null)
            {
                return BadRequest(ApiResponse<Enrollment>.Error("该学生已选过此课程"));
            }

            // 生成EID（选课编号）
            var maxEID = await _context.SMS_Enrollments.MaxAsync(e => (int?)e.EID) ?? 0;
            enrollment.EID = maxEID + 1;

            enrollment.CreateDate = DateTime.Now;
            enrollment.UpdateDate = DateTime.Now;
            enrollment.Status = "Active"; // 默认状态为活跃

            _context.SMS_Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.Id }, 
                ApiResponse<Enrollment>.Success(enrollment, "选课成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Enrollment>.Error($"选课失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 更新选课记录
    /// </summary>
    /// <param name="id">选课记录ID</param>
    /// <param name="enrollment">更新的选课信息</param>
    /// <returns>更新结果</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Enrollment>>> UpdateEnrollment(int id, Enrollment enrollment)
    {
        try
        {
            if (id != enrollment.Id)
            {
                return BadRequest(ApiResponse<Enrollment>.Error("ID不匹配"));
            }

            var existingEnrollment = await _context.SMS_Enrollments.FindAsync(id);
            if (existingEnrollment == null)
            {
                return NotFound(ApiResponse<Enrollment>.Error("选课记录不存在"));
            }

            existingEnrollment.Status = enrollment.Status;
            existingEnrollment.Semester = enrollment.Semester;
            existingEnrollment.Grade = enrollment.Grade;
            existingEnrollment.UpdateDate = DateTime.Now;
            existingEnrollment.UpdateUser = enrollment.UpdateUser;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Enrollment>.Success(existingEnrollment, "更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Enrollment>.Error($"更新失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 删除选课记录（退课）
    /// </summary>
    /// <param name="id">选课记录ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEnrollment(int id)
    {
        try
        {
            var enrollment = await _context.SMS_Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound(ApiResponse<object>.Error("选课记录不存在"));
            }

            _context.SMS_Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Success("退课成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error($"退课失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取学生选课统计报告
    /// </summary>
    /// <param name="studentSID">学生学号</param>
    /// <returns>学生选课统计</returns>
    [HttpGet("student/{studentSID}/report")]
    public async Task<ActionResult<ApiResponse<object>>> GetStudentEnrollmentReport(int studentSID)
    {
        try
        {
            var student = await _context.SMS_Students.FirstOrDefaultAsync(s => s.SID == studentSID);
            if (student == null)
            {
                return NotFound(ApiResponse<object>.Error("学生不存在"));
            }

            var enrollments = await _context.SMS_Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentSID == studentSID)
                .ToListAsync();

            var report = new
            {
                StudentInfo = new
                {
                    student.SID,
                    student.Name,
                    student.Major,
                    student.Email
                },
                Statistics = new
                {
                    TotalEnrollments = enrollments.Count,
                    ActiveEnrollments = enrollments.Count(e => e.Status == "Active"),
                    CompletedEnrollments = enrollments.Count(e => e.Status == "Completed"),
                    TotalCredits = enrollments.Where(e => e.Course != null).Sum(e => e.Course!.Credits),
                    AverageGrade = enrollments.Where(e => e.Grade.HasValue).Any() ? 
                        enrollments.Where(e => e.Grade.HasValue).Average(e => (double)e.Grade!.Value) : 0,
                    GPA = CalculateGPA(enrollments.Where(e => e.Grade.HasValue).Select(e => (double)e.Grade!.Value).ToList())
                },
                EnrollmentsBySemester = enrollments
                    .GroupBy(e => e.Semester)
                    .Select(g => new
                    {
                        Semester = g.Key,
                        Count = g.Count(),
                        Credits = g.Where(e => e.Course != null).Sum(e => e.Course!.Credits),
                        AverageGrade = g.Where(e => e.Grade.HasValue).Any() ? 
                            g.Where(e => e.Grade.HasValue).Average(e => (double)e.Grade!.Value) : 0
                    })
                    .OrderBy(x => x.Semester)
                    .ToList(),
                RecentEnrollments = enrollments
                    .OrderByDescending(e => e.CreateDate)
                    .Take(5)
                    .Select(e => new
                    {
                        e.EID,
                        e.Status,
                        e.Semester,
                        e.Grade,
                        CourseName = e.Course?.Name,
                        Credits = e.Course?.Credits
                    })
                    .ToList()
            };

            return Ok(ApiResponse<object>.Success(report));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error($"获取学生选课报告失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取课程选课统计报告
    /// </summary>
    /// <param name="courseCID">课程编号</param>
    /// <returns>课程选课统计</returns>
    [HttpGet("course/{courseCID}/report")]
    public async Task<ActionResult<ApiResponse<object>>> GetCourseEnrollmentReport(int courseCID)
    {
        try
        {
            var course = await _context.SMS_Courses.FirstOrDefaultAsync(c => c.CID == courseCID);
            if (course == null)
            {
                return NotFound(ApiResponse<object>.Error("课程不存在"));
            }

            var enrollments = await _context.SMS_Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseCID == courseCID)
                .ToListAsync();

            var report = new
            {
                CourseInfo = new
                {
                    course.CID,
                    course.Name,
                    course.Credits,
                    TeacherName = course.Teacher?.Name,
                    course.Description
                },
                Statistics = new
                {
                    TotalEnrollments = enrollments.Count,
                    ActiveEnrollments = enrollments.Count(e => e.Status == "Active"),
                    CompletedEnrollments = enrollments.Count(e => e.Status == "Completed"),
                    AverageGrade = enrollments.Where(e => e.Grade.HasValue).Any() ? 
                        enrollments.Where(e => e.Grade.HasValue).Average(e => (double)e.Grade!.Value) : 0,
                    PassRate = enrollments.Where(e => e.Grade.HasValue).Count(e => e.Grade!.Value >= 60.0m) * 100.0 / 
                              Math.Max(1, enrollments.Count(e => e.Grade.HasValue))
                },
                EnrollmentsBySemester = enrollments
                    .GroupBy(e => e.Semester)
                    .Select(g => new
                    {
                        Semester = g.Key,
                        Count = g.Count(),
                        AverageGrade = g.Where(e => e.Grade.HasValue).Any() ? 
                            g.Where(e => e.Grade.HasValue).Average(e => (double)e.Grade!.Value) : 0,
                        PassRate = g.Where(e => e.Grade.HasValue).Count(e => e.Grade!.Value >= 60.0m) * 100.0 / 
                                  Math.Max(1, g.Count(e => e.Grade.HasValue))
                    })
                    .OrderBy(x => x.Semester)
                    .ToList(),
                GradeDistribution = enrollments
                    .Where(e => e.Grade.HasValue)
                    .GroupBy(e => GetGradeLevel((double)e.Grade!.Value))
                    .Select(g => new
                    {
                        GradeLevel = g.Key,
                        Count = g.Count(),
                        Percentage = g.Count() * 100.0 / Math.Max(1, enrollments.Count(e => e.Grade.HasValue))
                    })
                    .ToList()
            };

            return Ok(ApiResponse<object>.Success(report));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error($"获取课程选课报告失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 批量更新成绩
    /// </summary>
    /// <param name="request">批量更新请求</param>
    /// <returns>更新结果</returns>
    [HttpPost("batch-update-grades")]
    public async Task<ActionResult<ApiResponse<object>>> BatchUpdateGrades([FromBody] BatchUpdateGradesRequest request)
    {
        try
        {
            var results = new List<object>();
            var successCount = 0;
            var failCount = 0;

            foreach (var gradeUpdate in request.GradeUpdates)
            {
                var enrollment = await _context.SMS_Enrollments.FindAsync(gradeUpdate.EnrollmentId);
                if (enrollment != null)
                {
                    enrollment.Grade = gradeUpdate.Grade.HasValue ? (decimal?)gradeUpdate.Grade.Value : null;
                    enrollment.UpdateDate = DateTime.Now;
                    enrollment.UpdateUser = request.UpdateUser;
                    successCount++;
                    
                    results.Add(new
                    {
                        EnrollmentId = gradeUpdate.EnrollmentId,
                        Status = "Success",
                        Message = "成绩更新成功"
                    });
                }
                else
                {
                    failCount++;
                    results.Add(new
                    {
                        EnrollmentId = gradeUpdate.EnrollmentId,
                        Status = "Failed",
                        Message = "选课记录不存在"
                    });
                }
            }

            await _context.SaveChangesAsync();

            var summary = new
            {
                Total = request.GradeUpdates.Count,
                Success = successCount,
                Failed = failCount,
                Results = results
            };

            return Ok(ApiResponse<object>.Success(summary, $"批量更新完成：成功 {successCount} 条，失败 {failCount} 条"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error($"批量更新成绩失败: {ex.Message}"));
        }
    }

    // 辅助方法
    private double CalculateGPA(List<double> grades)
    {
        if (!grades.Any()) return 0.0;

        var gpaSum = grades.Sum(grade =>
        {
            if (grade >= 90) return 4.0;
            if (grade >= 80) return 3.0;
            if (grade >= 70) return 2.0;
            if (grade >= 60) return 1.0;
            return 0.0;
        });

        return gpaSum / grades.Count;
    }

    private string GetGradeLevel(double grade)
    {
        if (grade >= 90) return "优秀 (90-100)";
        if (grade >= 80) return "良好 (80-89)";
        if (grade >= 70) return "中等 (70-79)";
        if (grade >= 60) return "及格 (60-69)";
        return "不及格 (<60)";
    }
}