

namespace LMS.Shared.DTOs.ModuleDtos
{
    public record ModuleDto(
        int Id,
        int CourseId,
        string Name,
        string Description,
        DateTime StartDate,
        DateTime EndDate);

}
