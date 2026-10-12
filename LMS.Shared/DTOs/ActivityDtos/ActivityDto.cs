

namespace LMS.Shared.DTOs.ActivityDtos
{
    public record ActivityDto(
        int Id,
        int ModeId,
        string Type,
        string Name,
        string Description,
        DateTime StartTime,
        DateTime EndTime
    );
    
    
}
