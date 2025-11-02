using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Models;
using SMS.Models.Api;
using System.ComponentModel.DataAnnotations;

namespace SMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly CimContext _context;

        public StudentsController(CimContext context)
        {
            _context = context;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<Student>>>> GetStudents([FromQuery] PagedRequest request)
        {
            try
            {
                var query = _context.SMS_Students.AsQueryable();

                // 搜索功能
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    query = query.Where(s => s.Name != null && s.Name.Contains(request.SearchTerm) || 
                                           s.SID.ToString().Contains(request.SearchTerm) || 
                                           s.Major != null && s.Major.Contains(request.SearchTerm) ||
                                           s.Email != null && s.Email.Contains(request.SearchTerm));
                }

                // 排序
                if (!string.IsNullOrEmpty(request.SortField))
                {
                    query = request.SortField.ToLower() switch
                    {
                        "name" => request.SortAscending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                        "sid" => request.SortAscending ? query.OrderBy(s => s.SID) : query.OrderByDescending(s => s.SID),
                        "major" => request.SortAscending ? query.OrderBy(s => s.Major) : query.OrderByDescending(s => s.Major),
                        "age" => request.SortAscending ? query.OrderBy(s => s.Age) : query.OrderByDescending(s => s.Age),
                        "email" => request.SortAscending ? query.OrderBy(s => s.Email) : query.OrderByDescending(s => s.Email),
                        _ => query.OrderBy(s => s.Id)
                    };
                }
                else
                {
                    query = query.OrderBy(s => s.Id);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(request.Skip)
                    .Take(request.PageSize)
                    .ToListAsync();

                var result = new PagedResult<Student>
                {
                    Items = items,
                    TotalCount = totalCount,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize
                };

                return Ok(ApiResponse<PagedResult<Student>>.Success(result, "获取学生列表成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<Student>>.Error($"获取学生列表失败: {ex.Message}"));
            }
        }

        // GET: api/Students/search
        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<PagedResult<Student>>>> SearchStudents([FromBody] StudentSearchRequest request)
        {
            try
            {
                var query = _context.SMS_Students.AsQueryable();

                // 按SID搜索
                if (!string.IsNullOrEmpty(request.SID))
                {
                    if (int.TryParse(request.SID, out int sidValue))
                    {
                        query = query.Where(s => s.SID == sidValue);
                    }
                }

                // 按姓名搜索
                if (!string.IsNullOrEmpty(request.Name))
                {
                    query = query.Where(s => s.Name != null && s.Name.Contains(request.Name));
                }

                // 按专业搜索
                if (!string.IsNullOrEmpty(request.Major))
                {
                    query = query.Where(s => s.Major != null && s.Major.Contains(request.Major));
                }

                // 按年龄范围搜索
                if (request.MinAge.HasValue)
                {
                    query = query.Where(s => s.Age >= request.MinAge.Value);
                }

                if (request.MaxAge.HasValue)
                {
                    query = query.Where(s => s.Age <= request.MaxAge.Value);
                }

                // 按邮箱搜索
                if (!string.IsNullOrEmpty(request.Email))
                {
                    query = query.Where(s => s.Email != null && s.Email.Contains(request.Email));
                }

                // 通用搜索词
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    query = query.Where(s => s.Name != null && s.Name.Contains(request.SearchTerm) || 
                                           s.SID.ToString().Contains(request.SearchTerm) || 
                                           s.Major != null && s.Major.Contains(request.SearchTerm) ||
                                           s.Email != null && s.Email.Contains(request.SearchTerm));
                }

                // 排序
                if (!string.IsNullOrEmpty(request.SortField))
                {
                    query = request.SortField.ToLower() switch
                    {
                        "name" => request.SortAscending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                        "sid" => request.SortAscending ? query.OrderBy(s => s.SID) : query.OrderByDescending(s => s.SID),
                        "major" => request.SortAscending ? query.OrderBy(s => s.Major) : query.OrderByDescending(s => s.Major),
                        "age" => request.SortAscending ? query.OrderBy(s => s.Age) : query.OrderByDescending(s => s.Age),
                        "email" => request.SortAscending ? query.OrderBy(s => s.Email) : query.OrderByDescending(s => s.Email),
                        _ => query.OrderBy(s => s.Id)
                    };
                }
                else
                {
                    query = query.OrderBy(s => s.Id);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(request.Skip)
                    .Take(request.PageSize)
                    .ToListAsync();

                var result = new PagedResult<Student>
                {
                    Items = items,
                    TotalCount = totalCount,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize
                };

                return Ok(ApiResponse<PagedResult<Student>>.Success(result, "搜索学生成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<Student>>.Error($"搜索学生失败: {ex.Message}"));
            }
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Student>>> GetStudent(int id)
        {
            try
            {
                var student = await _context.SMS_Students.FindAsync(id);

                if (student == null)
                {
                    return NotFound(ApiResponse<Student>.Error("学生不存在"));
                }

                return Ok(ApiResponse<Student>.Success(student, "获取学生信息成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Student>.Error($"获取学生信息失败: {ex.Message}"));
            }
        }

        // GET: api/Students/5/report
        [HttpGet("{id}/report")]
        public async Task<ActionResult<ApiResponse<object>>> GetStudentReport(int id)
        {
            try
            {
                var student = await _context.SMS_Students.FindAsync(id);
                if (student == null)
                {
                    return NotFound(ApiResponse<object>.Error("学生不存在"));
                }

                var enrollments = await _context.SMS_Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.StudentSID == student.SID)
                    .ToListAsync();

                var report = new
                {
                    Student = student,
                    TotalCourses = enrollments.Count,
                    TotalCredits = enrollments.Sum(e => e.Course?.Credits ?? 0),
                    AverageGrade = enrollments.Where(e => e.Grade.HasValue).Any() ? 
                        (double?)enrollments.Where(e => e.Grade.HasValue).Average(e => (double)e.Grade!.Value) : null,
                    Enrollments = enrollments.Select(e => new
                    {
                        e.Id,
                        CourseId = e.CourseCID,
                        CourseName = e.Course?.Name,
                        Credits = e.Course?.Credits,
                        Grade = e.Grade,
                        EnrollmentDate = e.CreateDate,
                        Status = e.Status
                    })
                };

                return Ok(ApiResponse<object>.Success(report, "获取学生报表成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"获取学生报表失败: {ex.Message}"));
            }
        }

        // POST: api/Students
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Student>>> PostStudent(Student student)
        {
            try
            {
                // 验证学号唯一性
                if (await _context.SMS_Students.AnyAsync(s => s.SID == student.SID))
                {
                    return BadRequest(ApiResponse<Student>.Error("学号已存在"));
                }

                _context.SMS_Students.Add(student);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetStudent", new { id = student.Id }, ApiResponse<Student>.Success(student, "创建学生成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Student>.Error($"创建学生失败: {ex.Message}"));
            }
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Student>>> PutStudent(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest(ApiResponse<Student>.Error("ID不匹配"));
            }

            try
            {
                // 验证学号唯一性（除自己外）
                if (await _context.SMS_Students.AnyAsync(s => s.SID == student.SID && s.Id != id))
                {
                    return BadRequest(ApiResponse<Student>.Error("学号已存在"));
                }

                _context.Entry(student).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<Student>.Success(student, "更新学生成功"));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await StudentExists(id))
                {
                    return NotFound(ApiResponse<Student>.Error("学生不存在"));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Student>.Error($"更新学生失败: {ex.Message}"));
            }
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteStudent(int id)
        {
            try
            {
                var student = await _context.SMS_Students.FindAsync(id);
                if (student == null)
                {
                    return NotFound(ApiResponse<object>.Error("学生不存在"));
                }

                // 检查是否有相关的选课记录
                var hasEnrollments = await _context.SMS_Enrollments.AnyAsync(e => e.StudentSID == student.SID);
                if (hasEnrollments)
                {
                    return BadRequest(ApiResponse<object>.Error("该学生有相关选课记录，无法删除"));
                }

                _context.SMS_Students.Remove(student);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success("删除成功", "删除学生成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"删除学生失败: {ex.Message}"));
            }
        }

        // POST: api/Students/batch
        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<object>>> BatchOperation([FromBody] BatchOperationRequest request)
        {
            try
            {
                if (request.StudentIds == null || !request.StudentIds.Any())
                {
                    return BadRequest(ApiResponse<object>.Error("请选择要操作的学生"));
                }

                var students = await _context.SMS_Students
                    .Where(s => request.StudentIds.Contains(s.Id))
                    .ToListAsync();

                if (!students.Any())
                {
                    return NotFound(ApiResponse<object>.Error("未找到指定的学生"));
                }

                switch (request.Operation?.ToLower())
                {
                    case "delete":
                        // 检查是否有学生有选课记录
                        var studentSIDs = students.Select(s => s.SID).ToList();
                        var hasEnrollments = await _context.SMS_Enrollments
                            .Where(e => studentSIDs.Contains(e.StudentSID))
                            .AnyAsync();

                        if (hasEnrollments)
                        {
                            return BadRequest(ApiResponse<object>.Error("部分学生有选课记录，无法删除"));
                        }

                        _context.SMS_Students.RemoveRange(students);
                        await _context.SaveChangesAsync();
                        return Ok(ApiResponse<object>.Success("批量删除成功", $"成功删除 {students.Count} 个学生"));

                    case "export":
                        // 导出功能可以在这里实现
                        var exportData = students.Select(s => new
                        {
                            s.SID,
                            s.Name,
                            s.Major,
                            s.Age,
                            s.Email
                        });
                        return Ok(ApiResponse<object>.Success(exportData, $"成功导出 {students.Count} 个学生"));

                    default:
                        return BadRequest(ApiResponse<object>.Error("不支持的操作"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"批量操作失败: {ex.Message}"));
            }
        }

        // GET: api/Students/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<object>>> GetStudentStatistics()
        {
            try
            {
                var totalCount = await _context.SMS_Students.CountAsync();
                var majorStats = await _context.SMS_Students
                    .GroupBy(s => s.Major)
                    .Select(g => new { Major = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                var ageStats = await _context.SMS_Students
                    .GroupBy(s => s.Age)
                    .Select(g => new { Age = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Age)
                    .ToListAsync();

                var enrollmentStats = await _context.SMS_Students
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.SID,
                        CourseCount = _context.SMS_Enrollments.Count(e => e.StudentSID == s.SID),
                        TotalCredits = _context.SMS_Enrollments
                            .Where(e => e.StudentSID == s.SID)
                            .Sum(e => e.Course != null ? e.Course.Credits : 0)
                    })
                    .OrderByDescending(x => x.CourseCount)
                    .Take(10)
                    .ToListAsync();

                var statistics = new
                {
                    TotalStudents = totalCount,
                    MajorDistribution = majorStats,
                    AgeDistribution = ageStats,
                    TopEnrolledStudents = enrollmentStats
                };

                return Ok(ApiResponse<object>.Success(statistics, "获取学生统计信息成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"获取学生统计信息失败: {ex.Message}"));
            }
        }

        private async Task<bool> StudentExists(int id)
        {
            return await _context.SMS_Students.AnyAsync(e => e.Id == id);
        }
    }

    // 学生搜索请求模型
    public class StudentSearchRequest : PagedRequest
    {
        public string? SID { get; set; }
        public string? Name { get; set; }
        public string? Major { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? Email { get; set; }
    }

    // 批量操作请求模型
    public class BatchOperationRequest
    {
        public List<int> StudentIds { get; set; } = new List<int>();
        public string? Operation { get; set; }
    }
}
