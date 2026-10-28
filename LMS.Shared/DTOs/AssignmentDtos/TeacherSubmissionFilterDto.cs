using LMS.Shared.Enums;

namespace LMS.Shared.DTOs.AssignmentDtos;

public sealed record TeacherSubmissionFilterDto
{
    public int? CourseId { get; init; }

    public int? ActivityId { get; init; }

    public string? StudentSearch { get; init; }

    public SubmissionStatus? Status { get; init; }
}