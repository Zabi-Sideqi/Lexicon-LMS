
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Services.Helpers;
using LMS.Shared.DTOs.DashboardDtos;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;

namespace LMS.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<TeacherDashboardDto> GetTeacherDashboardDtoAsync(
        string teacherId)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
        {
            throw new UnauthorizedAccessException(
                "A signed-in teacher is required.");
        }

        var teacher = await _userManager.FindByIdAsync(teacherId);

        if (teacher is null ||
            !await _userManager.IsInRoleAsync(teacher, "Teacher"))
        {
            throw new UnauthorizedAccessException(
                "Only teachers can view the teacher dashboard.");
        }

        var now = SchoolTime.FromUtc(DateTime.UtcNow);
        var today = now.Date;
        var tomorrow = today.AddDays(1);
        var upcomingCutoff = today.AddDays(8);

        // US02 requires all teachers to be able to see all courses.
        // Dashboard data must not depend on a course-teacher assignment.
        var courses = (
            await _unitOfWork.CourseRepository
                .GetAllAsync())
            .ToList();

        var activeCourses = new List<TeacherDashboardCourseDto>();
        var upcomingActivities = new List<TeacherDashboardActivityDto>();
        var todayActivities = new List<TeacherDashboardActivityDto>();
        var pendingFeedback = new List<TeacherDashboardSubmissionDto>();
        var lateSubmissions = new List<TeacherDashboardSubmissionDto>();
        var missingSubmissions =
            new List<TeacherDashboardMissingSubmissionDto>();

        foreach (var course in courses)
        {
            if (course.StartDate.Date <= today &&
                course.EndDate.Date >= today)
            {
                activeCourses.Add(new TeacherDashboardCourseDto(
                    course.Id,
                    course.Name,
                    course.StartDate,
                    course.EndDate));
            }

            var courseWithStudents =
                await _unitOfWork.CourseRepository
                    .GetCourseWithStudentsAsync(course.Id);

            var students = courseWithStudents?.Students.ToList() ?? [];

            var modules = await _unitOfWork.ModuleRepository
                .GetByCourseIdAsync(course.Id);

            var activities = new List<Activity>();

            foreach (var module in modules)
            {
                activities.AddRange(
                    await _unitOfWork.ActivityRepository
                        .GetByModuleIdAsync(module.Id));
            }

            foreach (var activity in activities)
            {
                var startTime = activity.StartTime;
                var endTime = activity.EndTime;

                var activityDto = new TeacherDashboardActivityDto(
                    activity.Id,
                    activity.Name,
                    course.Name,
                    startTime,
                    endTime);

                if (startTime >= today && startTime < tomorrow)
                {
                    todayActivities.Add(activityDto);
                }

                if (startTime >= now && startTime < upcomingCutoff)
                {
                    upcomingActivities.Add(activityDto);
                }
            }

            var submissions = (
                await _unitOfWork.SubmissionRepository
                    .GetByCourseIdAsync(course.Id))
                .ToList();

            foreach (var submission in submissions)
            {
                var submittedAt =
                    SchoolTime.FromUtc(submission.SubmittedAt);

                var dueDate = submission.Activity.EndTime;

                var hasFeedback = submission.Feedbacks.Any();

                var dto = new TeacherDashboardSubmissionDto(
                    submission.Id,
                    submission.ActivityId,
                    submission.Activity.Name,
                    course.Name,
                    submission.StudentId,
                    submission.Student.UserName
                        ?? submission.Student.Email
                        ?? "Okänd student",
                    submission.FileName,
                    submittedAt,
                    dueDate,
                    hasFeedback);

                if (!hasFeedback)
                {
                    pendingFeedback.Add(dto);
                }

                if (submission.SubmittedAt != default &&
                    submittedAt > dueDate)
                {
                    lateSubmissions.Add(dto);
                }
            }

            foreach (var assignment in activities.Where(activity =>
                         activity.Type == ActivityType.Assignment &&
                         activity.EndTime < now))
            {
                var studentsWithSubmission = submissions
                    .Where(submission =>
                        submission.ActivityId == assignment.Id)
                    .Select(submission => submission.StudentId)
                    .ToHashSet(StringComparer.Ordinal);

                foreach (var student in students.Where(student =>
                             !studentsWithSubmission.Contains(student.Id)))
                {
                    missingSubmissions.Add(
                        new TeacherDashboardMissingSubmissionDto(
                            assignment.Id,
                            assignment.Name,
                            course.Name,
                            student.Id,
                            student.UserName
                                ?? student.Email
                                ?? "Okänd student",
                            assignment.EndTime));
                }
            }
        }

        return new TeacherDashboardDto(
            activeCourses
                .OrderBy(course => course.EndDate)
                .ToList(),
            upcomingActivities
                .OrderBy(activity => activity.StartTime)
                .ToList(),
            todayActivities
                .OrderBy(activity => activity.StartTime)
                .ToList(),
            pendingFeedback
                .OrderBy(submission => submission.SubmittedAt)
                .ToList(),
            lateSubmissions
                .OrderByDescending(submission => submission.SubmittedAt)
                .ToList(),
            missingSubmissions
                .OrderBy(submission => submission.DueDate)
                .ToList());
    }
}
