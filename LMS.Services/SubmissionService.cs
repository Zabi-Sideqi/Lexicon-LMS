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

    public async Task<IEnumerable<StudentAssignmentDto>> GetMyAssignmentsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user?.CourseId is null)
            return [];

        // Two queries in total (not one per assignment): the assignments of the
        // student's course, and all submissions made by the student.
        var assignments = await _uow.ActivityRepository
            .GetAssignmentsByCourseIdAsync(user.CourseId.Value);

        var submissions = await _uow.SubmissionRepository
            .GetByStudentIdAsync(userId);

        // If a student has handed in more than once for the same assignment,
        // the latest submission is used.
        var latestSubmissionByActivity = submissions
            .GroupBy(submission => submission.ActivityId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(submission => submission.SubmittedAt).First());

        var now = SchoolTime.FromUtc(DateTime.UtcNow);

        return assignments
            .Select(assignment =>
            {
                latestSubmissionByActivity.TryGetValue(assignment.Id, out var submission);
                DateTime? submittedAt = submission is null
                ? null
                : SchoolTime.FromUtc(submission.SubmittedAt);

                return new StudentAssignmentDto(
                    assignment.Id,
                    assignment.Name,
                    assignment.Description,
                    assignment.EndTime,
                    submittedAt,
                    SubmissionStatusCalculator.Calculate(assignment.EndTime, submittedAt, now),
                    false); // Feedback does not exist yet (US11)
            })
            .ToList();
    }

    public async Task<StudentAssignmentDto?> GetMyAssignmentAsync(string userId, int assignmentId)
    {
        // Only assignments in the student's own course are returned,
        // so a student cannot read another course by changing the id.
        var assignments = await GetMyAssignmentsAsync(userId);

        return assignments.FirstOrDefault(assignment => assignment.Id == assignmentId);
    }
}