using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SMS.Models;

public enum CourseStatus
{
    未开课 = 0,
    正在进行 = 1,
    已结束 = 2
}

[Table("SMS_Courses")]
public class Course
{
    public int Id { get; set; }
    public int CID { get; set; }
    public string? Name { get; set; }
    public int Credits { get; set; }
    public string? Description { get; set; }
    
    /// <summary>
    /// 课程状态：未开课、正在进行、已结束
    /// </summary>
    public CourseStatus CourseStatus { get; set; } = CourseStatus.未开课;
    
    /// <summary>
    /// 授课教师ID - 外键关联到Teacher表
    /// </summary>
    public int? TeacherId { get; set; }
    
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string? CreateUser { get; set; }
    public string? UpdateUser { get; set; }

    // 导航属性
    /// <summary>
    /// 授课教师
    /// </summary>
    [ForeignKey("TeacherId")]
    public virtual Teacher? Teacher { get; set; }

    /// <summary>
    /// 该课程的所有选课记录
    /// </summary>
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

