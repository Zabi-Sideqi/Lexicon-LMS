
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.FeedbackDtos;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;

namespace LMS.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public FeedbackService(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task<IEnumerable<FeedbackDto>> GetBySubmissionAsync(
        int submissionId,
        string studentId)
    {
        var submission = await _uow.SubmissionRepository
            .GetByIdAsync(submissionId);

        if (submission is null)
        {
            throw new KeyNotFoundException(
                $"Submission with id {submissionId} was not found.");
        }

        if (submission.StudentId != studentId)
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to read feedback for this submission.");
        }

        var feedbackItems = await _uow.FeedbackRepository
            .GetBySubmissionIdAsync(submissionId);

        var result = new List<FeedbackDto>();

        foreach (var feedback in feedbackItems)
        {
            var teacher = await _userManager.FindByIdAsync(
                feedback.TeacherId);

            result.Add(new FeedbackDto(
                feedback.Id,
                feedback.SubmissionId,
                feedback.Text,
                feedback.CreatedAt,
                feedback.TeacherId,
                teacher?.UserName ?? "Okänd lärare"));
        }

        return result;
    }

    public async Task<FeedbackDto> CreateAsync(
        int submissionId,
        string teacherId,
        FeedbackForCreationDto dto)
    {
        var submission = await _uow.SubmissionRepository
            .GetByIdAsync(submissionId);

        if (submission is null)
        {
            throw new KeyNotFoundException(
                $"Submission with id {submissionId} was not found.");
        }

        var teacher = await _userManager.FindByIdAsync(teacherId);

        if (teacher is null ||
            !await _userManager.IsInRoleAsync(teacher, "Teacher"))
        {
            throw new UnauthorizedAccessException(
                "Only a teacher can create feedback.");
        }

        // All users with the Teacher role may provide feedback on any course.
        // The role check above is the authorization rule for this action.

        if (dto is null || string.IsNullOrWhiteSpace(dto.Text))
        {
            throw new ArgumentException(
                "Feedback text cannot be empty.");
        }

        var feedback = new Feedback
        {
            SubmissionId = submissionId,
            TeacherId = teacherId,
            Text = dto.Text.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _uow.FeedbackRepository.Create(feedback);
        await _uow.SaveChangesAsync();
        var notification = new Notification
        {
            StudentId = submission.StudentId,
            FeedbackId = feedback.Id,
            Message = "You have received new feedback on your submission.",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _uow.NotificationRepository.Create(notification);
        await _uow.SaveChangesAsync();

        return new FeedbackDto(
            feedback.Id,
            feedback.SubmissionId,
            feedback.Text,
            feedback.CreatedAt,
            feedback.TeacherId,
            teacher.UserName ?? "Okänd lärare");
    }
}
