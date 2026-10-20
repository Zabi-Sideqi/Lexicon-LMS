
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Services.Helpers;
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;

namespace LMS.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubmissionService(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task<IEnumerable<StudentAssignmentDto>> GetMyAssignmentsAsync(
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user?.CourseId is null)
            return [];

        var assignments = await _uow.ActivityRepository
            .GetAssignmentsByCourseIdAsync(user.CourseId.Value);

        var submissions = await _uow.SubmissionRepository
            .GetByStudentIdAsync(userId);

        // Use the latest submission for each assignment.
        var latestSubmissionByActivity = submissions
            .GroupBy(submission => submission.ActivityId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(submission => submission.SubmittedAt)
                    .First());

        var now = SchoolTime.FromUtc(DateTime.UtcNow);
        var result = new List<StudentAssignmentDto>();

        foreach (var assignment in assignments)
        {
            latestSubmissionByActivity.TryGetValue(
                assignment.Id,
                out var submission);

            DateTime? submittedAt = submission is null
                ? null
                : SchoolTime.FromUtc(submission.SubmittedAt);

            var hasFeedback = false;

            if (submission is not null)
            {
                var feedbackItems = await _uow.FeedbackRepository
                    .GetBySubmissionIdAsync(submission.Id);

                hasFeedback = feedbackItems.Any();
            }

            result.Add(new StudentAssignmentDto(
                assignment.Id,
                assignment.Name,
                assignment.Description,
                assignment.EndTime,
                submittedAt,
                SubmissionStatusCalculator.Calculate(
                    assignment.EndTime,
                    submittedAt,
                    now),
                hasFeedback,
                submission?.Id));
        }

        return result;
    }

    public async Task<StudentAssignmentDto?> GetMyAssignmentAsync(
        string userId,
        int assignmentId)
    {
        var assignments = await GetMyAssignmentsAsync(userId);

        return assignments.FirstOrDefault(
            assignment => assignment.Id == assignmentId);
    }

    public async Task<IEnumerable<TeacherSubmissionDto>> GetSubmissionsForTeacherAsync(
        string teacherId)
    {
        var teacher = await _userManager.FindByIdAsync(teacherId);

        if (teacher is null ||
            !await _userManager.IsInRoleAsync(teacher, "Teacher"))
        {
            throw new UnauthorizedAccessException(
                "Only teachers can view course submissions.");
        }

        if (teacher.CourseId is null)
            return [];

        var submissions = await _uow.SubmissionRepository
            .GetByCourseIdAsync(teacher.CourseId.Value);

        return submissions
            .Select(submission => new TeacherSubmissionDto(
                submission.Id,
                submission.ActivityId,
                submission.Activity.Name,
                submission.StudentId,
                submission.Student.UserName
                    ?? submission.Student.Email
                    ?? "Okänd student",
                submission.FileName,
                SchoolTime.FromUtc(submission.SubmittedAt),
                submission.Feedbacks.Any()))
            .ToList();
    }

}
