
namespace LMS.Shared.DTOs.ActivityDtos;

public record ActivityDto(
    int Id,
    int ModuleId,
    string Type,
    string Name,
    string Description,
    DateTime StartTime,
    DateTime EndTime);
