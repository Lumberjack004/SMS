using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    [Table("SMS_Enrollments")]
    public class Enrollment
    {
        public int Id { get; set; }

        /// <summary>
        /// 选课编号 - 业务主键
        /// </summary>
        public int EID { get; set; }

        /// <summary>
        /// 学生学号 - 外键关联到Student表的SID
        /// </summary>
        [Required]
        public int StudentSID { get; set; }

        /// <summary>
        /// 课程编号 - 外键关联到Course表的CID
        /// </summary>
        [Required]
        public int CourseCID { get; set; }

        /// <summary>
        /// 选课状态
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// 学期
        /// </summary>
        [StringLength(50)]
        public string? Semester { get; set; }

        /// <summary>
        /// 成绩
        /// </summary>
        [Range(0, 100)]
        public decimal? Grade { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? CreateUser { get; set; }
        public string? UpdateUser { get; set; }

        // 导航属性
        [ForeignKey("StudentSID")]
        public virtual Student? Student { get; set; }

        [ForeignKey("CourseCID")]
        public virtual Course? Course { get; set; }
    }
}
