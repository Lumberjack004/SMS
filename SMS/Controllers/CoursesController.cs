using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Models;

namespace SMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly CimContext _context;

    public CoursesController(CimContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有课程信息
    /// </summary>
    /// <returns>课程列表</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetCourses()
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
                c.CreateDate,
                Teacher = c.Teacher != null ? new
                {
                    c.Teacher.Id,
                    c.Teacher.TID,
                    c.Teacher.Name
                } : null,
                StudentsCount = c.Enrollments.Count()
            })
            .ToListAsync();

        return Ok(courses);
    }

    /// <summary>
    /// 根据ID获取特定课程信息
    /// </summary>
    /// <param name="id">课程ID</param>
    /// <returns>课程信息</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetCourse(int id)
    {
        var course = await _context.SMS_Courses
            .Include(c => c.Teacher)
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.CID,
                c.Name,
                c.Credits,
                c.Description,
                c.CreateDate,
                c.UpdateDate,
                c.CreateUser,
                c.UpdateUser,
                Teacher = c.Teacher != null ? new
                {
                    c.Teacher.Id,
                    c.Teacher.TID,
                    c.Teacher.Name,
                    c.Teacher.Email
                } : null,
                StudentsCount = c.Enrollments.Count()
            })
            .FirstOrDefaultAsync();

        if (course == null)
        {
            return NotFound(new { message = "课程不存在" });
        }

        return Ok(course);
    }

    /// <summary>
    /// 根据课程编号获取课程信息
    /// </summary>
    /// <param name="cid">课程编号</param>
    /// <returns>课程信息</returns>
    [HttpGet("by-cid/{cid}")]
    public async Task<ActionResult<object>> GetCourseByCID(int cid)
    {
        var course = await _context.SMS_Courses
            .Include(c => c.Teacher)
            .Where(c => c.CID == cid)
            .Select(c => new
            {
                c.Id,
                c.CID,
                c.Name,
                c.Credits,
                c.Description,
                c.CreateDate,
                Teacher = c.Teacher != null ? new
                {
                    c.Teacher.Id,
                    c.Teacher.TID,
                    c.Teacher.Name
                } : null,
                StudentsCount = c.Enrollments.Count()
            })
            .FirstOrDefaultAsync();

        if (course == null)
        {
            return NotFound(new { message = "课程不存在" });
        }

        return Ok(course);
    }

    /// <summary>
    /// 创建新课程
    /// </summary>
    /// <param name="course">课程信息</param>
    /// <returns>创建的课程信息</returns>
    [HttpPost]
    public async Task<ActionResult<Course>> CreateCourse(Course course)
    {
        // 检查课程编号是否已存在
        var existingCourse = await _context.SMS_Courses.FirstOrDefaultAsync(c => c.CID == course.CID);
        if (existingCourse != null)
        {
            return BadRequest(new { message = "课程编号已存在" });
        }

        // 如果指定了教师ID，检查教师是否存在
        if (course.TeacherId.HasValue)
        {
            var teacher = await _context.SMS_Teachers.FindAsync(course.TeacherId);
            if (teacher == null)
            {
                return BadRequest(new { message = "指定的教师不存在" });
            }
        }

        course.CreateDate = DateTime.Now;
        course.UpdateDate = DateTime.Now;

        _context.SMS_Courses.Add(course);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
    }

    /// <summary>
    /// 更新课程信息
    /// </summary>
    /// <param name="id">课程ID</param>
    /// <param name="course">更新的课程信息</param>
    /// <returns>更新结果</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, Course course)
    {
        if (id != course.Id)
        {
            return BadRequest(new { message = "ID不匹配" });
        }

        var existingCourse = await _context.SMS_Courses.FindAsync(id);
        if (existingCourse == null)
        {
            return NotFound(new { message = "课程不存在" });
        }

        // 检查课程编号是否被其他课程使用
        var courseWithSameCID = await _context.SMS_Courses
            .FirstOrDefaultAsync(c => c.CID == course.CID && c.Id != id);
        if (courseWithSameCID != null)
        {
            return BadRequest(new { message = "课程编号已被其他课程使用" });
        }

        // 如果指定了教师ID，检查教师是否存在
        if (course.TeacherId.HasValue)
        {
            var teacher = await _context.SMS_Teachers.FindAsync(course.TeacherId);
            if (teacher == null)
            {
                return BadRequest(new { message = "指定的教师不存在" });
            }
        }

        // 更新字段
        existingCourse.CID = course.CID;
        existingCourse.Name = course.Name;
        existingCourse.Credits = course.Credits;
        existingCourse.Description = course.Description;
        existingCourse.TeacherId = course.TeacherId;
        existingCourse.UpdateDate = DateTime.Now;
        existingCourse.UpdateUser = course.UpdateUser;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CourseExists(id))
            {
                return NotFound(new { message = "课程不存在" });
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    /// <summary>
    /// 删除课程
    /// </summary>
    /// <param name="id">课程ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.SMS_Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound(new { message = "课程不存在" });
        }

        // 检查是否有学生选课
        var hasEnrollments = await _context.SMS_Enrollments.AnyAsync(e => e.CourseCID == course.CID);
        if (hasEnrollments)
        {
            return BadRequest(new { message = "该课程已有学生选课，无法删除" });
        }

        _context.SMS_Courses.Remove(course);
        await _context.SaveChangesAsync();

        return Ok(new { message = "课程删除成功" });
    }

    /// <summary>
    /// 获取课程的选课学生
    /// </summary>
    /// <param name="id">课程ID</param>
    /// <returns>学生列表</returns>
    [HttpGet("{id}/students")]
    public async Task<ActionResult<IEnumerable<object>>> GetCourseStudents(int id)
    {
        var course = await _context.SMS_Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound(new { message = "课程不存在" });
        }

        var students = await _context.SMS_Enrollments
            .Where(e => e.CourseCID == course.CID)
            .Include(e => e.Student)
            .Select(e => new
            {
                EnrollmentId = e.Id,
                e.Status,
                e.Semester,
                e.Grade,
                Student = e.Student != null ? new
                {
                    e.Student.Id,
                    e.Student.SID,
                    e.Student.Name,
                    e.Student.Email,
                    e.Student.Major
                } : null
            })
            .ToListAsync();

        return Ok(students);
    }

    private bool CourseExists(int id)
    {
        return _context.SMS_Courses.Any(e => e.Id == id);
    }
}
