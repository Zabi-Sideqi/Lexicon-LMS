using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.DashboardDtos;
using LMS.Shared.DTOs.DocumentDtos;
using LMS.Shared.DTOs.FeedbackDtos;
using LMS.Shared.DTOs.NotificationDtos;
using LMS.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;

namespace LMS.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISubmissionService _submissionService;

    public DashboardService(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        ISubmissionService submissionService)
    {
        _uow = uow;
        _userManager = userManager;
        _submissionService = submissionService;
    }

    public async Task<StudentDashboardDto> GetStudentDashboardAsync(string studentId)
    {
        var user = await _userManager.FindByIdAsync(studentId);
        if (user?.CourseId is null)
            throw new ArgumentException("Student is not enrolled in a course.");

        var courseId = user.CourseId.Value;

        var course = await _uow.CourseRepository.GetByIdAsync(courseId);
        var courseDto = course is null
            ? null
            : new StudentCourseDto(
                course.Id, course.Name, course.Description,
                course.StartDate, course.EndDate);

        var activities = await _uow.ActivityRepository.GetByCourseIdAsync(courseId);
        var upcomingActivities = activities
            .Where(a => a.StartTime >= DateTime.UtcNow)
            .OrderBy(a => a.StartTime)
            .Take(5)
            .Select(MapToActivityDto)
            .ToList();

        var allAssignments = await _submissionService.GetMyAssignmentsAsync(studentId);
        var pendingAssignments = allAssignments
            .Where(a => a.Status == SubmissionStatus.NotSubmitted
                     || a.Status == SubmissionStatus.Late
                     || a.Status == SubmissionStatus.SubmittedLate)
            .Take(5)
            .ToList();

        var documents = await _uow.DocumentRepository.GetByCourseIdAsync(courseId);
        var recentDocuments = documents
            .OrderByDescending(d => d.UploadedAt)
            .Take(5)
            .Select(MapToDocumentDto)
            .ToList();

        var feedback = await _uow.FeedbackRepository.GetByStudentIdAsync(studentId);
        var recentFeedback = feedback
            .OrderByDescending(f => f.CreatedAt)
            .Take(3)
            .Select(MapToFeedbackDto)
            .ToList();

        var notifications = await _uow.NotificationRepository.GetByStudentIdAsync(studentId);
        var unreadNotifications = notifications
            .Where(n => !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new NotificationDto(n.Id, n.Message, n.CreatedAt, n.IsRead))
            .ToList();

        return new StudentDashboardDto(
            courseDto,
            upcomingActivities,
            pendingAssignments,
            recentDocuments,
            recentFeedback,
            unreadNotifications);
    }

    private static ActivityDto MapToActivityDto(Activity a) =>
        new(a.Id, a.ModuleId, a.Type.ToString(), a.Name, a.Description, a.StartTime, a.EndTime);

    private static DocumentDto MapToDocumentDto(Document d) =>
        new(d.Id, d.FileName, d.ContentType, d.FileSize, d.UploadedAt, d.UploadedById,
            d.CourseId, d.Course?.Name,
            d.ModuleId, d.Module?.Name,
            d.ActivityId, d.Activity?.Name,
            null);

    private static FeedbackDto MapToFeedbackDto(Feedback f) =>
        new(f.Id, f.SubmissionId, f.Text, f.CreatedAt, f.TeacherId, f.Teacher?.UserName ?? "Okänd lärare");
}