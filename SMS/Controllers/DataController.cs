using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Models;
using SMS.Models.Api;
using System.Text;
using System.Text.Json;

namespace SMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly CimContext _context;

    public DataController(CimContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 导出学生数据为JSON
    /// </summary>
    /// <returns>学生数据JSON文件</returns>
    [HttpGet("export/students")]
    public async Task<IActionResult> ExportStudents()
    {
        var students = await _context.SMS_Students.ToListAsync();
        var json = JsonSerializer.Serialize(students, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"students_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
    }

    /// <summary>
    /// 导出教师数据为JSON
    /// </summary>
    /// <returns>教师数据JSON文件</returns>
    [HttpGet("export/teachers")]
    public async Task<IActionResult> ExportTeachers()
    {
        var teachers = await _context.SMS_Teachers.ToListAsync();
        var json = JsonSerializer.Serialize(teachers, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"teachers_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
    }

    /// <summary>
    /// 导出课程数据为JSON
    /// </summary>
    /// <returns>课程数据JSON文件</returns>
    [HttpGet("export/courses")]
    public async Task<IActionResult> ExportCourses()
    {
        var courses = await _context.SMS_Courses
            .Include(c => c.Teacher)
            .Select(c => new
            {
                c.Id,
                c.CID,
                c.Name,
                c.Credits,
                c.Description,
                c.TeacherId,
                TeacherName = c.Teacher != null ? c.Teacher.Name : null
            })
            .ToListAsync();

        var json = JsonSerializer.Serialize(courses, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"courses_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
    }

    /// <summary>
    /// 导出选课数据为JSON
    /// </summary>
    /// <returns>选课数据JSON文件</returns>
    [HttpGet("export/enrollments")]
    public async Task<IActionResult> ExportEnrollments()
    {
        var enrollments = await _context.SMS_Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Select(e => new
            {
                e.Id,
                StudentSID = e.StudentSID,
                CourseCID = e.CourseCID,
                CreateDate = e.CreateDate,
                e.Grade,
                StudentName = e.Student != null ? e.Student.Name : null,
                CourseName = e.Course != null ? e.Course.Name : null
            })
            .ToListAsync();

        var json = JsonSerializer.Serialize(enrollments, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"enrollments_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
    }

    /// <summary>
    /// 导出完整数据库备份
    /// </summary>
    /// <returns>完整数据备份JSON文件</returns>
    [HttpGet("export/full-backup")]
    public async Task<IActionResult> ExportFullBackup()
    {
        var backup = new
        {
            ExportDate = DateTime.Now,
            Students = await _context.SMS_Students.ToListAsync(),
            Teachers = await _context.SMS_Teachers.ToListAsync(),
            Courses = await _context.SMS_Courses.ToListAsync(),
            Enrollments = await _context.SMS_Enrollments.ToListAsync()
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"full_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
    }

    /// <summary>
    /// 批量导入学生数据
    /// </summary>
    /// <param name="students">学生数据列表</param>
    /// <returns>导入结果</returns>
    [HttpPost("import/students")]
    public async Task<ActionResult<ApiResponse<object>>> ImportStudents([FromBody] List<Student> students)
    {
        if (students == null || students.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Error("学生数据不能为空"));
        }

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var student in students)
        {
            try
            {
                // 检查学号是否已存在
                if (await _context.SMS_Students.AnyAsync(s => s.SID == student.SID))
                {
                    errors.Add($"学号 {student.SID} 已存在，跳过");
                    errorCount++;
                    continue;
                }

                _context.SMS_Students.Add(student);
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"导入学生 {student.SID} 失败: {ex.Message}");
                errorCount++;
            }
        }

        if (successCount > 0)
        {
            await _context.SaveChangesAsync();
        }

        var result = new
        {
            TotalCount = students.Count,
            SuccessCount = successCount,
            ErrorCount = errorCount,
            Errors = errors
        };

        return Ok(ApiResponse<object>.Success(result, $"导入完成：成功 {successCount} 条，失败 {errorCount} 条"));
    }

    /// <summary>
    /// 批量导入教师数据
    /// </summary>
    /// <param name="teachers">教师数据列表</param>
    /// <returns>导入结果</returns>
    [HttpPost("import/teachers")]
    public async Task<ActionResult<ApiResponse<object>>> ImportTeachers([FromBody] List<Teacher> teachers)
    {
        if (teachers == null || teachers.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Error("教师数据不能为空"));
        }

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var teacher in teachers)
        {
            try
            {
                // 检查教师ID是否已存在
                if (await _context.SMS_Teachers.AnyAsync(t => t.TID == teacher.TID))
                {
                    errors.Add($"教师ID {teacher.TID} 已存在，跳过");
                    errorCount++;
                    continue;
                }

                _context.SMS_Teachers.Add(teacher);
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"导入教师 {teacher.TID} 失败: {ex.Message}");
                errorCount++;
            }
        }

        if (successCount > 0)
        {
            await _context.SaveChangesAsync();
        }

        var result = new
        {
            TotalCount = teachers.Count,
            SuccessCount = successCount,
            ErrorCount = errorCount,
            Errors = errors
        };

        return Ok(ApiResponse<object>.Success(result, $"导入完成：成功 {successCount} 条，失败 {errorCount} 条"));
    }

    /// <summary>
    /// 批量更新学生成绩
    /// </summary>
    /// <param name="gradeUpdates">成绩更新数据</param>
    /// <returns>更新结果</returns>
    [HttpPost("batch/update-grades")]
    public async Task<ActionResult<ApiResponse<object>>> BatchUpdateGrades([FromBody] List<GradeUpdateRequest> gradeUpdates)
    {
        if (gradeUpdates == null || gradeUpdates.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Error("成绩数据不能为空"));
        }

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var update in gradeUpdates)
        {
            try
            {
                var enrollment = await _context.SMS_Enrollments
                    .FirstOrDefaultAsync(e => e.StudentSID == update.SID && e.CourseCID == update.CID);

                if (enrollment == null)
                {
                    errors.Add($"未找到学生 {update.SID} 的课程 {update.CID} 选课记录");
                    errorCount++;
                    continue;
                }

                enrollment.Grade = string.IsNullOrEmpty(update.Grade) ? null : decimal.Parse(update.Grade);
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"更新学生 {update.SID} 课程 {update.CID} 成绩失败: {ex.Message}");
                errorCount++;
            }
        }

        if (successCount > 0)
        {
            await _context.SaveChangesAsync();
        }

        var result = new
        {
            TotalCount = gradeUpdates.Count,
            SuccessCount = successCount,
            ErrorCount = errorCount,
            Errors = errors
        };

        return Ok(ApiResponse<object>.Success(result, $"成绩更新完成：成功 {successCount} 条，失败 {errorCount} 条"));
    }

    /// <summary>
    /// 清理数据（谨慎使用）
    /// </summary>
    /// <param name="tableNames">要清理的表名列表</param>
    /// <returns>清理结果</returns>
    [HttpDelete("cleanup")]
    public async Task<ActionResult<ApiResponse<object>>> CleanupData([FromBody] List<string> tableNames)
    {
        if (tableNames == null || tableNames.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Error("请指定要清理的表"));
        }

        var result = new Dictionary<string, int>();

        foreach (var tableName in tableNames)
        {
            switch (tableName.ToLower())
            {
                case "enrollments":
                    var enrollmentCount = await _context.SMS_Enrollments.CountAsync();
                    _context.SMS_Enrollments.RemoveRange(_context.SMS_Enrollments);
                    result["Enrollments"] = enrollmentCount;
                    break;

                case "courses":
                    var courseCount = await _context.SMS_Courses.CountAsync();
                    _context.SMS_Courses.RemoveRange(_context.SMS_Courses);
                    result["Courses"] = courseCount;
                    break;

                case "students":
                    var studentCount = await _context.SMS_Students.CountAsync();
                    _context.SMS_Students.RemoveRange(_context.SMS_Students);
                    result["Students"] = studentCount;
                    break;

                case "teachers":
                    var teacherCount = await _context.SMS_Teachers.CountAsync();
                    _context.SMS_Teachers.RemoveRange(_context.SMS_Teachers);
                    result["Teachers"] = teacherCount;
                    break;

                default:
                    result[tableName] = -1; // 表示未找到
                    break;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Success(result, "数据清理完成"));
    }
}

/// <summary>
/// 成绩更新请求模型
/// </summary>
public class GradeUpdateRequest
{
    /// <summary>
    /// 学生ID
    /// </summary>
    public int SID { get; set; }

    /// <summary>
    /// 课程号
    /// </summary>
    public int CID { get; set; }

    /// <summary>
    /// 成绩
    /// </summary>
    public string? Grade { get; set; }
}
