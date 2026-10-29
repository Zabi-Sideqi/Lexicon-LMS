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
using LMS.Services.Helpers;
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
            return new StudentDashboardDto(null, [], [], [], [], []);

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


    public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(string teacherId)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
            throw new UnauthorizedAccessException("A signed-in teacher is required.");

        var teacher = await _userManager.FindByIdAsync(teacherId);

        if (teacher is null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
            throw new UnauthorizedAccessException("Only teachers can view their dashboard.");

        var now = SchoolTime.FromUtc(DateTime.UtcNow);
        var courses = (await _uow.CourseRepository.GetByTeacherIdAsync(teacherId))
            .OrderBy(course => course.StartDate)
            .ToList();

        var courseDtos = new List<TeacherDashboardCourseDto>();
        var upcomingActivities = new List<TeacherDashboardActivityDto>();
        var recentSubmissions = new List<TeacherDashboardSubmissionDto>();
        var attentionItems = new List<TeacherDashboardAttentionDto>();

        var totalSubmissions = 0;
        var pendingFeedbackCount = 0;
        var lateSubmissionCount = 0;
        var missingSubmissionCount = 0;
        var totalStudentIds = new HashSet<string>();

        foreach (var course in courses)
        {
            var courseWithStudents =
                await _uow.CourseRepository.GetCourseWithStudentsAsync(course.Id);

            var students = courseWithStudents?.Students.ToList() ?? [];

            foreach (var student in students)
                totalStudentIds.Add(student.Id);

            var activities = (await _uow.ActivityRepository
                .GetByCourseIdAsync(course.Id)).ToList();

            var courseUpcoming = activities
                .Where(activity => activity.StartTime >= now)
                .OrderBy(activity => activity.StartTime)
                .ToList();

            upcomingActivities.AddRange(courseUpcoming.Select(activity =>
                new TeacherDashboardActivityDto(
                    activity.Id,
                    activity.Name,
                    activity.Type.ToString(),
                    course.Id,
                    course.Name,
                    activity.StartTime,
                    activity.EndTime)));

            courseDtos.Add(new TeacherDashboardCourseDto(
                course.Id,
                course.Name,
                course.StartDate,
                course.EndDate,
                students.Count,
                courseUpcoming.Count,
                course.StartDate.Date <= now.Date &&
                course.EndDate.Date >= now.Date));

            var courseSubmissions = (await _uow.SubmissionRepository
                .GetByCourseIdAsync(course.Id)).ToList();

            totalSubmissions += courseSubmissions.Count;

            pendingFeedbackCount += courseSubmissions.Count(
                submission => !submission.Feedbacks.Any());

            foreach (var submission in courseSubmissions)
            {
                var submittedAt = SchoolTime.FromUtc(submission.SubmittedAt);
                var isLate = submittedAt > submission.Activity.EndTime;



                recentSubmissions.Add(new TeacherDashboardSubmissionDto(
                    submission.Id,
                    submission.Student.UserName
                        ?? submission.Student.Email
                        ?? "Unknown student",
                    submission.Activity.Name,
                    course.Name,
                    submittedAt,
                    isLate,
                    submission.Feedbacks.Any()));
            }

            var assignments = activities
                .Where(activity => activity.Type == ActivityType.Assignment)
                .ToList();

            foreach (var assignment in assignments)
            {
                foreach (var student in students)
                {
                    var latestSubmission = courseSubmissions
                        .Where(submission =>
                            submission.ActivityId == assignment.Id &&
                            submission.StudentId == student.Id)
                        .OrderByDescending(submission => submission.SubmittedAt)
                        .FirstOrDefault();

                    if (latestSubmission is not null)
                    {
                        var submittedAt =
                            SchoolTime.FromUtc(latestSubmission.SubmittedAt);

                        if (submittedAt > assignment.EndTime)
                        {
                            lateSubmissionCount++;

                            attentionItems.Add(new TeacherDashboardAttentionDto(
                                assignment.Id,
                                assignment.Name,
                                course.Id,
                                course.Name,
                                student.Id,
                                student.UserName ?? student.Email ?? "Unknown student",
                                assignment.EndTime,
                                "Late submission"));
                        }

                        continue;
                    }

                    if (assignment.EndTime < now)
                    {
                        missingSubmissionCount++;

                        attentionItems.Add(new TeacherDashboardAttentionDto(
                            assignment.Id,
                            assignment.Name,
                            course.Id,
                            course.Name,
                            student.Id,
                            student.UserName ?? student.Email ?? "Unknown student",
                            assignment.EndTime,
                            "Missing submission"));
                    }
                }
            }
        }

        return new TeacherDashboardDto(
            courseDtos,
            upcomingActivities
                .OrderBy(activity => activity.StartTime)
                .Take(5)
                .ToList(),
            recentSubmissions
                .OrderByDescending(submission => submission.SubmittedAt)
                .Take(5)
                .ToList(),
            attentionItems
                .OrderBy(item => item.DueAt)
                .Take(8)
                .ToList(),
            totalSubmissions,
            pendingFeedbackCount,
            lateSubmissionCount,
            missingSubmissionCount,
            totalStudentIds.Count);
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
