namespace LMS.Shared.DTOs.DocumentDtos;

public sealed record DocumentDto(
    int Id,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAt,
    string UploadedById,
    int? CourseId,
    int? ModuleId,
    int? ActivityId);