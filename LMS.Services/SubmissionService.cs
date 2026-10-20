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
    private readonly IFileStorage _fileStorage;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubmissionService(
        IUnitOfWork uow,
        IFileStorage fileStorage,
        UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _fileStorage = fileStorage;
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

    public async Task<StudentAssignmentDto> SubmitAsync(
        string userId,
        int assignmentId,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize)
    {
        FileValidator.Validate(fileName, fileSize);

        // The assignment must be in the student's own course.
        // Anything else gets the same "not found", so nothing about other courses is revealed.
        var assignment = await GetMyAssignmentAsync(userId, assignmentId);

        if (assignment is null)
            throw new KeyNotFoundException($"Assignment with id {assignmentId} not found.");

        // The stored name is random, so nobody can guess or overwrite a file by its name.
        var storedFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(fileName);
        await _fileStorage.SaveAsync(fileStream, storedFileName);

        var submission = new Submission
        {
            ActivityId = assignmentId,
            StudentId = userId,
            FileName = fileName,
            StoredFileName = storedFileName,
            ContentType = contentType,
            FileSize = fileSize,
            SubmittedAt = DateTime.UtcNow // set by the system, never by the client
        };

        _uow.SubmissionRepository.Create(submission);
        await _uow.SaveChangesAsync();

        // A late submission gets the status "SubmittedLate" from the status calculation.
        return (await GetMyAssignmentAsync(userId, assignmentId))!;
    }
}
