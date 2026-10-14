
using LMS.Shared.DTOs.ModuleDtos;

namespace Service.Contracts;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetAllAsync();

    Task<ModuleDto?> GetByIdAsync(int id);

    Task<IEnumerable<ModuleDto>> GetByCourseIdAsync(int courseId);

    Task<ModuleDto> CreateAsync(ModuleForCreationDto dto);

    Task UpdateAsync(int id, ModuleForUpdateDto dto);

    Task DeleteAsync(int id);
}
