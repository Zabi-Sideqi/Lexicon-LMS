
namespace LMS.Shared.DTOs.DashboardDtos;

public sealed record TeacherDashboardDto(
    IReadOnlyCollection<TeacherDashboardCourseDto> ActiveCourses,
    IReadOnlyCollection<TeacherDashboardActivityDto> UpcomingActivities,
    IReadOnlyCollection<TeacherDashboardActivityDto> TodayActivities,
    IReadOnlyCollection<TeacherDashboardSubmissionDto> PendingFeedback,
    IReadOnlyCollection<TeacherDashboardSubmissionDto> LateSubmissions,
    IReadOnlyCollection<TeacherDashboardMissingSubmissionDto> MissingSubmissions
);

public sealed record TeacherDashboardCourseDto(
    int CourseId,
    string CourseName,
    DateTime StartDate,
    DateTime EndDate
);

public sealed record TeacherDashboardActivityDto(
    int ActivityId,
    string ActivityName,
    string CourseName,
    DateTime StartTime,
    DateTime EndTime
);

public sealed record TeacherDashboardSubmissionDto(
    int SubmissionId,
    int ActivityId,
    string ActivityName,
    string CourseName,
    string StudentId,
    string StudentName,
    string FileName,
    DateTime SubmittedAt,
    DateTime DueDate,
    bool HasFeedback
);

public sealed record TeacherDashboardMissingSubmissionDto(
    int ActivityId,
    string ActivityName,
    string CourseName,
    string StudentId,
    string StudentName,
    DateTime DueDate
);
