namespace LMS.Shared.DTOs.DocumentDtos;

public sealed record DocumentDto(
    int Id,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAt,
    string UploadedByUserId,
    int? CourseId,
    int? ModuleId);