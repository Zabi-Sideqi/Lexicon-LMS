namespace LMS.Shared.DTOs.NotificationDtos;

public sealed record NotificationDto(
    int Id,
    string Message,
    DateTime CreatedAt,
    bool IsRead);