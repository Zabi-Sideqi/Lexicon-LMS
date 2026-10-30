
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.CommonDtos;

namespace Service.Contracts;

public interface IActivityService
{
    Task<IEnumerable<ActivityDto>> GetAllAsync();

    Task<ActivityDto?> GetByIdAsync(int id);

    Task<IEnumerable<ActivityDto>> GetByModuleIdAsync(int moduleId);

    Task<ActivityDto> CreateAsync(ActivityCreateDto dto);

    Task UpdateAsync(int id, ActivityUpdateDto dto);

    Task DeleteAsync(int id);

    Task<PagedList<ActivityDto>> GetPagedAsync(
        PageParameters pageParameters,
        int? courseId = null);
}
