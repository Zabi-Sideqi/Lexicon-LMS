using LMS.Shared.Enums;

namespace LMS.Shared.DTOs.AssignmentDtos;

public record StudentAssignmentDto(
    int Id,
    string Name,
    string Description,
    DateTime Deadline,
    DateTime? SubmittedAt,
    string? SubmittedFileName,
    SubmissionStatus Status,
    bool HasFeedback,
    int? SubmissionId);