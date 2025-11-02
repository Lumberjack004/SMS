namespace SMS.Models.Api
{
    // 统一的API响应格式
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object? Errors { get; set; }

        public static ApiResponse<T> Success(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Error(string message, object? errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors
            };
        }
    }

    // 分页请求模型
    public class PagedRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortField { get; set; }
        public bool SortAscending { get; set; } = true;
        public int PageNumber { get; set; } = 1;

        public int Skip => (Page - 1) * PageSize;
    }

    // 分页结果模型
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    // 学生搜索请求模型
    public class StudentSearchRequest : PagedRequest
    {
        public string? Keyword { get; set; }
        public string? Major { get; set; }
        public string? Grade { get; set; }
        public string? Status { get; set; }
        public int? SID { get; set; }
        public string? Name { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
    }

    // 教师搜索请求模型
    public class TeacherSearchRequest : PagedRequest
    {
        public string? Keyword { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public int? TID { get; set; }
        public string? Name { get; set; }
    }

    // 课程搜索请求模型
    public class CourseSearchRequest : PagedRequest
    {
        public string? Keyword { get; set; }
        public string? Department { get; set; }
        public int? Credits { get; set; }
        public string? Status { get; set; }
        public int? TeacherId { get; set; }
    }

    // 选课搜索请求模型
    public class EnrollmentSearchRequest : PagedRequest
    {
        public int? StudentId { get; set; }
        public int? CourseId { get; set; }
        public int? StudentSID { get; set; }
        public int? CourseCID { get; set; }
        public string? StudentName { get; set; }
        public string? CourseName { get; set; }
        public string? TeacherName { get; set; }
        public string? Status { get; set; }
        public string? Grade { get; set; }
        public string? Semester { get; set; }
        public double? MinGrade { get; set; }
        public double? MaxGrade { get; set; }
        public DateTime? EnrollmentDateFrom { get; set; }
        public DateTime? EnrollmentDateTo { get; set; }
    }

    // 批量更新成绩请求模型
    public class BatchUpdateGradesRequest
    {
        public List<GradeUpdateItem> Grades { get; set; } = new List<GradeUpdateItem>();
        public List<GradeUpdateItem> GradeUpdates { get; set; } = new List<GradeUpdateItem>();
        public string? UpdateUser { get; set; }
    }

    public class GradeUpdateItem
    {
        public int EnrollmentId { get; set; }
        public double? Grade { get; set; }
    }

    // 用户搜索请求模型
    public class UserSearchRequest : PagedRequest
    {
        public string? Keyword { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
    }

    // 教师申请搜索请求模型
    public class TeacherApplicationSearchRequest : PagedRequest
    {
        public string? Keyword { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmittedDateFrom { get; set; }
        public DateTime? SubmittedDateTo { get; set; }
    }
}
