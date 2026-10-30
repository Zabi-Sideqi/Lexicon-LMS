using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Services.Helpers;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;
using LMS.Shared.DTOs.CommonDtos;

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
                submission?.FileName, // SubmittedFileName
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
        string teacherId,
        TeacherSubmissionFilterDto? filter = null)
    {
        var teacher = await _userManager.FindByIdAsync(teacherId);

        if (teacher is null ||
            !await _userManager.IsInRoleAsync(teacher, "Teacher"))
        {
            throw new UnauthorizedAccessException(
                "Only teachers can view course submissions.");
        }

// US02: All teachers can view submissions from all courses.
// US13: Allow filtering submissions by course.
var courses = await _uow.CourseRepository.GetAllAsync();

if (filter?.CourseId is int courseId)
{
    courses = courses
        .Where(course => course.Id == courseId)
        .ToList();
}

        if (!courses.Any())
            return [];

        var now = SchoolTime.FromUtc(DateTime.UtcNow);
        var result = new List<TeacherSubmissionDto>();

        foreach (var course in courses)
        {
            var courseWithStudents = await _uow.CourseRepository
                .GetCourseWithStudentsAsync(course.Id);

            if (courseWithStudents is null)
                continue;

            var assignments = await _uow.ActivityRepository
                .GetAssignmentsByCourseIdAsync(course.Id);

            var courseSubmissions = await _uow.SubmissionRepository
                .GetByCourseIdAsync(course.Id);

            var latestSubmissionByStudentAndActivity = courseSubmissions
                .GroupBy(submission => (submission.StudentId, submission.ActivityId))
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(submission => submission.SubmittedAt)
                        .First());

            foreach (var assignment in assignments)
            {
                foreach (var student in courseWithStudents.Students)
                {
                    latestSubmissionByStudentAndActivity.TryGetValue(
                        (student.Id, assignment.Id),
                        out var submission);

                    DateTime? submittedAt = submission is null
                        ? null
                        : SchoolTime.FromUtc(submission.SubmittedAt);

                    result.Add(new TeacherSubmissionDto(
                        submission?.Id,
                        assignment.Id,
                        assignment.Name,
                        student.Id,
                        student.UserName ?? student.Email ?? "Okänd student",
                        submission?.FileName,
                        submittedAt,
                        assignment.EndTime,
                        SubmissionStatusCalculator.Calculate(
                            assignment.EndTime,
                            submittedAt,
                            now),
                        submission?.Feedbacks.Any() ?? false));
                }
            }
        }

        IEnumerable<TeacherSubmissionDto> filtered = result;

        if (filter?.ActivityId is int activityId)
        {
            filtered = filtered.Where(
                submission => submission.ActivityId == activityId);
        }

        if (!string.IsNullOrWhiteSpace(filter?.StudentSearch))
        {
            filtered = filtered.Where(submission =>
                submission.StudentName.Contains(
                    filter.StudentSearch,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (filter?.Status is SubmissionStatus statusFilter)
        {
            filtered = filtered.Where(
                submission => submission.Status == statusFilter);
        }

        return filtered.ToList();
    }

    public async Task<(Stream Stream, string FileName, string ContentType)>
    DownloadSubmissionAsync(string teacherId, int submissionId)
    {
        var teacher = await _userManager.FindByIdAsync(teacherId);

        if (teacher is null ||
            !await _userManager.IsInRoleAsync(teacher, "Teacher"))
        {
            throw new UnauthorizedAccessException(
                "Only teachers can download submissions.");
        }

        var submission = await _uow.SubmissionRepository
            .GetByIdAsync(submissionId);

        if (submission is null)
        {
            throw new KeyNotFoundException(
                $"Submission with id {submissionId} was not found.");
        }

        var stream = await _fileStorage.OpenReadAsync(
            submission.StoredFileName);

        return (stream, submission.FileName, submission.ContentType);
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

        // US14: Notify the teachers assigned to the course.
        var student = await _userManager.FindByIdAsync(userId);

        if (student?.CourseId is int courseId)
        {
            var course = await _uow.CourseRepository
                .GetByIdAsync(courseId);

            if (course is not null)
            {
                foreach (var teacher in course.Teachers)
                {
                    _uow.NotificationRepository.Create(new Notification
                    {
                        RecipientId = teacher.Id,
                        Message = $"Studenten {student.UserName ?? student.Email ?? userId} har lämnat in uppgiften: {assignment.Name}.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                }

                await _uow.SaveChangesAsync();
            }
        }

        // Return the updated assignment information.
        return (await GetMyAssignmentAsync(userId, assignmentId))!;

    }

    public async Task<PagedList<TeacherSubmissionDto>> GetSubmissionsForTeacherPagedAsync(
    string teacherId,
    PageParameters pageParameters,
    bool? hasFeedback = null)
    {
        var teacher = await _userManager.FindByIdAsync(teacherId);
        if (teacher is null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
            throw new UnauthorizedAccessException("Only teachers can view submissions.");

        var paged = await _uow.SubmissionRepository.GetPagedAsync(pageParameters, hasFeedback);

        var now = SchoolTime.FromUtc(DateTime.UtcNow);
        var dtos = paged.Items.Select(s =>
        {
            var submittedAt = SchoolTime.FromUtc(s.SubmittedAt);

            return new TeacherSubmissionDto(
                s.Id,
                s.ActivityId,
                s.Activity.Name,
                s.StudentId,
                s.Student.UserName ?? "Okänd",
                s.FileName,
                submittedAt,
                s.Activity.EndTime,
                SubmissionStatusCalculator.Calculate(
                    s.Activity.EndTime,
                    submittedAt,
                    now),
                s.Feedbacks.Any());
        }).ToList();

        return new PagedList<TeacherSubmissionDto>(
            dtos, paged.PageNumber, paged.PageSize, paged.TotalCount, paged.TotalPages);
    }
}
