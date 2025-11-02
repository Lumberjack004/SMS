using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Models;
using System.Text;

namespace SMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly CimContext _context;

    public ExportController(CimContext context)
    {
        _context = context;
    }

    // 导出学生数据为CSV
    [HttpGet("students")]
    public async Task<IActionResult> ExportStudentsToCSV()
    {
        try
        {
            var students = await _context.SMS_Students.ToListAsync();
            
            var csv = new StringBuilder();
            csv.AppendLine("学号,姓名,年龄,专业,邮箱,创建时间,更新时间");
            
            foreach (var student in students)
            {
                csv.AppendLine($"{student.SID},{EscapeCSV(student.Name ?? "")},{student.Age},{EscapeCSV(student.Major ?? "")},{EscapeCSV(student.Email ?? "")},{student.CreateDate:yyyy-MM-dd HH:mm:ss},{student.UpdateDate:yyyy-MM-dd HH:mm:ss}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"学生数据_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"导出学生数据失败: {ex.Message}");
        }
    }

    // 导出教师数据为CSV
    [HttpGet("teachers")]
    public async Task<IActionResult> ExportTeachersToCSV()
    {
        try
        {
            var teachers = await _context.SMS_Teachers.ToListAsync();
            
            var csv = new StringBuilder();
            csv.AppendLine("教师号,姓名,年龄,专业,邮箱,创建时间,更新时间");
            
            foreach (var teacher in teachers)
            {
                csv.AppendLine($"{teacher.TID},{EscapeCSV(teacher.Name ?? "")},{teacher.Age},{EscapeCSV(teacher.Major ?? "")},{EscapeCSV(teacher.Email ?? "")},{teacher.CreateDate:yyyy-MM-dd HH:mm:ss},{teacher.UpdateDate:yyyy-MM-dd HH:mm:ss}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"教师数据_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"导出教师数据失败: {ex.Message}");
        }
    }

    // 导出课程数据为CSV
    [HttpGet("courses")]  
    public async Task<IActionResult> ExportCoursesToCSV()
    {
        try
        {
            var courses = await _context.SMS_Courses.Include(c => c.Teacher).ToListAsync();
            
            var csv = new StringBuilder();
            csv.AppendLine("课程号,课程名,学分,教师,描述,课程状态,创建时间,更新时间");
            
            foreach (var course in courses)
            {
                csv.AppendLine($"{course.CID},{EscapeCSV(course.Name ?? "")},{course.Credits},{EscapeCSV(course.Teacher?.Name ?? "未分配")},{EscapeCSV(course.Description ?? "")},{course.CourseStatus},{course.CreateDate:yyyy-MM-dd HH:mm:ss},{course.UpdateDate:yyyy-MM-dd HH:mm:ss}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"课程数据_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"导出课程数据失败: {ex.Message}");
        }
    }

    // 导出选课数据为CSV
    [HttpGet("enrollments")]
    public async Task<IActionResult> ExportEnrollmentsToCSV()
    {
        try
        {
            var enrollments = await _context.SMS_Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .ToListAsync();
            
            var csv = new StringBuilder();
            csv.AppendLine("选课ID,学生学号,学生姓名,课程号,课程名,成绩,创建时间,更新时间");
            
            foreach (var enrollment in enrollments)
            {
                csv.AppendLine($"{enrollment.EID},{enrollment.Student?.SID},{EscapeCSV(enrollment.Student?.Name ?? "")},{enrollment.Course?.CID},{EscapeCSV(enrollment.Course?.Name ?? "")},{enrollment.Grade?.ToString() ?? ""},{enrollment.CreateDate:yyyy-MM-dd HH:mm:ss},{enrollment.UpdateDate:yyyy-MM-dd HH:mm:ss}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"选课数据_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"导出选课数据失败: {ex.Message}");
        }
    }

    // CSV转义辅助方法
    private string EscapeCSV(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        
        return field;
    }
}
