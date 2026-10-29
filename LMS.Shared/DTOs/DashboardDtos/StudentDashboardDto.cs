using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.DocumentDtos;
using LMS.Shared.DTOs.FeedbackDtos;
using LMS.Shared.DTOs.NotificationDtos;

namespace LMS.Shared.DTOs.DashboardDtos;

public sealed record StudentDashboardDto(
    StudentCourseDto? Course,
    IEnumerable<ActivityDto> UpcomingActivities,
    IEnumerable<StudentAssignmentDto> PendingAssignments,
    IEnumerable<DocumentDto> RecentDocuments,
    IEnumerable<FeedbackDto> RecentFeedback,
    IEnumerable<NotificationDto> UnreadNotifications);