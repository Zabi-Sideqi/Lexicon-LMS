using LMS.Shared.Enums;

namespace LMS.Shared.DTOs.AssignmentDtos;

public sealed record TeacherSubmissionDto(
    int? SubmissionId,
    int ActivityId,
    string ActivityName,
    string StudentId,
    string StudentName,
    string? FileName,
    DateTime? SubmittedAt,
    DateTime Deadline,
    SubmissionStatus Status,
    bool HasFeedback);