using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Models;
using SMS.Models.Api;

namespace SMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly CimContext _context;

        public TeachersController(CimContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取教师列表（分页）
        /// </summary>
        /// <param name="request">分页请求参数</param>
        /// <returns>教师列表</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<Teacher>>>> GetTeachers([FromQuery] PagedRequest request)
        {
            try
            {
                var query = _context.SMS_Teachers.AsQueryable();

                // 搜索功能
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(t =>
                        t.TID.ToString().Contains(request.SearchTerm) ||
                        (t.Name != null && t.Name.Contains(request.SearchTerm)) ||
                        (t.Major != null && t.Major.Contains(request.SearchTerm)) ||
                        (t.Email != null && t.Email.Contains(request.SearchTerm)));
                }

                // 排序功能
                if (!string.IsNullOrWhiteSpace(request.SortField))
                {
                    query = request.SortField.ToLower() switch
                    {
                        "tid" => request.SortAscending ? query.OrderBy(t => t.TID) : query.OrderByDescending(t => t.TID),
                        "name" => request.SortAscending ? query.OrderBy(t => t.Name) : query.OrderByDescending(t => t.Name),
                        "major" => request.SortAscending ? query.OrderBy(t => t.Major) : query.OrderByDescending(t => t.Major),
                        "age" => request.SortAscending ? query.OrderBy(t => t.Age) : query.OrderByDescending(t => t.Age),
                        _ => query.OrderBy(t => t.TID)
                    };
                }
                else
                {
                    query = query.OrderBy(t => t.TID);
                }

                var totalCount = await query.CountAsync();

                var teachers = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var result = new PagedResult<Teacher>
                {
                    Items = teachers,
                    TotalCount = totalCount,
                    CurrentPage = request.PageNumber,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Ok(ApiResponse<PagedResult<Teacher>>.Success(result, "获取教师列表成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<Teacher>>.Error($"获取教师列表失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 高级搜索教师
        /// </summary>
        /// <param name="request">高级搜索请求参数</param>
        /// <returns>搜索结果</returns>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<PagedResult<Teacher>>>> SearchTeachers([FromQuery] TeacherSearchRequest request)
        {
            try
            {
                var query = _context.SMS_Teachers.AsQueryable();

                // 按教师ID搜索
                if (request.TID.HasValue)
                {
                    query = query.Where(t => t.TID == request.TID.Value);
                }

                // 按姓名搜索
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    query = query.Where(t => t.Name != null && t.Name.Contains(request.Name));
                }

                // 按专业搜索
                if (!string.IsNullOrWhiteSpace(request.Department))
                {
                    query = query.Where(t => t.Major != null && t.Major.Contains(request.Department));
                }

                // 按职位搜索 (用 Major 代替 Title)
                if (!string.IsNullOrWhiteSpace(request.Title))
                {
                    query = query.Where(t => t.Major != null && t.Major.Contains(request.Title));
                }

                // 通用搜索词
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(t =>
                        t.TID.ToString().Contains(request.SearchTerm) ||
                        (t.Name != null && t.Name.Contains(request.SearchTerm)) ||
                        (t.Major != null && t.Major.Contains(request.SearchTerm)) ||
                        (t.Email != null && t.Email.Contains(request.SearchTerm)));
                }

                var totalCount = await query.CountAsync();

                var teachers = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var result = new PagedResult<Teacher>
                {
                    Items = teachers,
                    TotalCount = totalCount,
                    CurrentPage = request.PageNumber,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Ok(ApiResponse<PagedResult<Teacher>>.Success(result, "搜索教师成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<Teacher>>.Error($"搜索教师失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 根据ID获取特定教师信息
        /// </summary>
        /// <param name="id">教师ID</param>
        /// <returns>教师信息</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Teacher>>> GetTeacher(int id)
        {
            try
            {
                var teacher = await _context.SMS_Teachers.FindAsync(id);

                if (teacher == null)
                {
                    return NotFound(ApiResponse<Teacher>.Error("教师不存在"));
                }

                return Ok(ApiResponse<Teacher>.Success(teacher, "获取教师信息成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Teacher>.Error($"获取教师信息失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取教师报告
        /// </summary>
        /// <param name="id">教师ID</param>
        /// <returns>教师报告</returns>
        [HttpGet("{id}/report")]
        public async Task<ActionResult<ApiResponse<object>>> GetTeacherReport(int id)
        {
            try
            {
                var teacher = await _context.SMS_Teachers.FindAsync(id);
                if (teacher == null)
                {
                    return NotFound(ApiResponse<object>.Error("教师不存在"));
                }

                var courses = await _context.SMS_Courses
                    .Include(c => c.Enrollments)
                    .Where(c => c.TeacherId == id)
                    .ToListAsync();

                var totalCourses = courses.Count;
                var totalStudents = courses.Sum(c => c.Enrollments.Count);
                var totalCredits = courses.Sum(c => c.Credits);

                var report = new
                {
                    TeacherInfo = teacher,
                    TotalCourses = totalCourses,
                    TotalStudents = totalStudents,
                    TotalCredits = totalCredits,
                    CourseDetails = courses.Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Credits,
                        c.CID,
                        EnrolledStudents = c.Enrollments.Count,
                        AverageGrade = c.Enrollments.Where(e => e.Grade.HasValue).Any() 
                            ? c.Enrollments.Where(e => e.Grade.HasValue).Average(e => e.Grade) 
                            : null
                    }).ToList()
                };

                return Ok(ApiResponse<object>.Success(report, "获取教师报告成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"获取教师报告失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 创建新教师
        /// </summary>
        /// <param name="teacher">教师信息</param>
        /// <returns>创建的教师信息</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Teacher>>> CreateTeacher(Teacher teacher)
        {
            try
            {
                // 检查教师ID是否已存在
                var existingTeacher = await _context.SMS_Teachers.FirstOrDefaultAsync(t => t.TID == teacher.TID);
                if (existingTeacher != null)
                {
                    return BadRequest(ApiResponse<Teacher>.Error("教师ID已存在"));
                }

                teacher.CreateDate = DateTime.Now;
                _context.SMS_Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id },
                    ApiResponse<Teacher>.Success(teacher, "教师创建成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Teacher>.Error($"创建教师失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新教师信息
        /// </summary>
        /// <param name="id">教师ID</param>
        /// <param name="teacher">更新的教师信息</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateTeacher(int id, Teacher teacher)
        {
            try
            {
                if (id != teacher.Id)
                {
                    return BadRequest(ApiResponse<object>.Error("ID不匹配"));
                }

                var duplicateTeacher = await _context.SMS_Teachers
                    .FirstOrDefaultAsync(t => t.TID == teacher.TID && t.Id != id);
                if (duplicateTeacher != null)
                {
                    return BadRequest(ApiResponse<object>.Error("教师ID已被其他教师使用"));
                }

                var existingTeacher = await _context.SMS_Teachers.FindAsync(id);
                if (existingTeacher != null)
                {
                    existingTeacher.Name = teacher.Name;
                    existingTeacher.TID = teacher.TID;
                    existingTeacher.Major = teacher.Major;
                    existingTeacher.Age = teacher.Age;
                    existingTeacher.Email = teacher.Email;
                    existingTeacher.UpdateDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(ApiResponse<object>.Error("教师不存在"));
                }

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success(new { }, "教师信息更新成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"更新教师信息失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 删除教师
        /// </summary>
        /// <param name="id">教师ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteTeacher(int id)
        {
            try
            {
                var teacher = await _context.SMS_Teachers.FindAsync(id);
                if (teacher == null)
                {
                    return NotFound(ApiResponse<object>.Error("教师不存在"));
                }

                // 检查是否有相关课程
                var hasCourses = await _context.SMS_Courses.AnyAsync(c => c.TeacherId == id);
                if (hasCourses)
                {
                    return BadRequest(ApiResponse<object>.Error("该教师有授课任务，无法删除"));
                }

                _context.SMS_Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success(new { }, "教师删除成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"删除教师失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 批量删除教师
        /// </summary>
        /// <param name="ids">教师ID数组</param>
        /// <returns>删除结果</returns>
        [HttpDelete("batch")]
        public async Task<ActionResult<ApiResponse<object>>> BatchDeleteTeachers([FromBody] int[] ids)
        {
            try
            {
                var teachers = await _context.SMS_Teachers.Where(t => ids.Contains(t.Id)).ToListAsync();
                if (teachers.Count == 0)
                {
                    return NotFound(ApiResponse<object>.Error("未找到要删除的教师"));
                }

                // 检查是否有教师有授课任务
                var teacherIds = teachers.Select(t => t.Id).ToList();
                var hasCoursesTeachers = await _context.SMS_Courses
                    .Where(c => c.TeacherId.HasValue && teacherIds.Contains(c.TeacherId.Value))
                    .Select(c => c.TeacherId)
                    .Distinct()
                    .ToListAsync();

                if (hasCoursesTeachers.Any())
                {
                    return BadRequest(ApiResponse<object>.Error("部分教师有授课任务，无法删除"));
                }

                _context.SMS_Teachers.RemoveRange(teachers);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success(new { }, $"成功删除 {teachers.Count} 位教师"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"批量删除教师失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取教师工作量统计
        /// </summary>
        /// <returns>教师工作量统计</returns>
        [HttpGet("workload-stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetTeacherWorkloadStats()
        {
            try
            {
                var workloadStats = await _context.SMS_Teachers
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        t.TID,
                        t.Major,
                        CourseCount = _context.SMS_Courses.Count(c => c.TeacherId == t.Id),
                        StudentCount = _context.SMS_Courses
                            .Where(c => c.TeacherId == t.Id)
                            .SelectMany(c => c.Enrollments)
                            .Count(),
                        TotalCredits = _context.SMS_Courses
                            .Where(c => c.TeacherId == t.Id)
                            .Sum(c => c.Credits)
                    })
                    .OrderByDescending(x => x.CourseCount)
                    .ToListAsync();

                return Ok(ApiResponse<object>.Success(workloadStats, "获取教师工作量成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"获取教师工作量失败: {ex.Message}"));
            }
        }
    }
}
