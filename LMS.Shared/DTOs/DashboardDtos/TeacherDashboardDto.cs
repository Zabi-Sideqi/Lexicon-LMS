
namespace LMS.Shared.DTOs.DashboardDtos;

public sealed record TeacherDashboardDto(
    IReadOnlyCollection<TeacherDashboardCourseDto> Courses,
    IReadOnlyCollection<TeacherDashboardActivityDto> UpcomingActivities,
    IReadOnlyCollection<TeacherDashboardSubmissionDto> RecentSubmissions,
    IReadOnlyCollection<TeacherDashboardAttentionDto> AttentionItems,
    int TotalSubmissions,
    int PendingFeedbackCount,
    int LateSubmissionCount,
    int MissingSubmissionCount,
    int StudentCount);

public sealed record TeacherDashboardCourseDto(
    int Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    int StudentCount,
    int UpcomingActivityCount,
    bool IsActive);

public sealed record TeacherDashboardActivityDto(
    int Id,
    string Name,
    string Type,
    int CourseId,
    string CourseName,
    DateTime StartTime,
    DateTime EndTime);

public sealed record TeacherDashboardSubmissionDto(
    int SubmissionId,
    string StudentName,
    string ActivityName,
    string CourseName,
    DateTime SubmittedAt,
    bool IsLate,
    bool HasFeedback);

public sealed record TeacherDashboardAttentionDto(
    int ActivityId,
    string ActivityName,
    int CourseId,
    string CourseName,
    string StudentId,
    string StudentName,
    DateTime DueAt,
    string Status);
