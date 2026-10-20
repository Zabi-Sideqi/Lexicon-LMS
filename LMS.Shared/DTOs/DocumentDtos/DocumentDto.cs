namespace LMS.Shared.DTOs.DocumentDtos;

public sealed record DocumentDto(
    int Id,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAt,
    string UploadedById,
    int? CourseId,
    string? CourseName,
    int? ModuleId,
    string? ModuleName,
    int? ActivityId,
    string? ActivityName,
    string? AttachPath);