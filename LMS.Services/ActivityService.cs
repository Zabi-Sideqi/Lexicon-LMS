
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.CommonDtos;
using Service.Contracts;

namespace LMS.Services;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _uow;

    public ActivityService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<ActivityDto>> GetAllAsync()
    {
        var activities = await _uow.ActivityRepository.GetAllAsync();

        return activities
            .OrderBy(activity => activity.StartTime)
            .Select(MapToDto);
    }

    public async Task<ActivityDto?> GetByIdAsync(int id)
    {
        var activity = await _uow.ActivityRepository.GetByIdAsync(id);

        return activity is null ? null : MapToDto(activity);
    }

    public async Task<IEnumerable<ActivityDto>> GetByModuleIdAsync(
        int moduleId)
    {
        var module = await _uow.ModuleRepository.GetByIdAsync(moduleId);

        if (module is null)
        {
            throw new ResourceNotFoundException(
                $"Module with id {moduleId} not found.");
        }

        var activities = await _uow.ActivityRepository
            .GetByModuleIdAsync(moduleId);

        return activities
            .OrderBy(activity => activity.StartTime)
            .Select(MapToDto);
    }

    public async Task<ActivityDto> CreateAsync(ActivityCreateDto dto)
    {
        var module = await _uow.ModuleRepository
            .GetByIdAsync(dto.ModuleId);

        if (module is null)
        {
            throw new ResourceNotFoundException(
                $"Module with id {dto.ModuleId} not found.");
        }

        ValidateTimes(dto.StartTime, dto.EndTime);

        ValidateModulePeriod(
            dto.StartTime,
            dto.EndTime,
            module.StartDate,
            module.EndDate);

        var type = ParseActivityType(dto.Type);

        await ValidateNoOverlapAsync(
            dto.ModuleId,
            dto.StartTime,
            dto.EndTime);

        var activity = new Activity
        {
            ModuleId = dto.ModuleId,
            Type = type,
            Name = dto.Name,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

        _uow.ActivityRepository.Create(activity);
        await _uow.SaveChangesAsync();

        return MapToDto(activity);
    }

    public async Task UpdateAsync(int id, ActivityUpdateDto dto)
    {
        var activity = await _uow.ActivityRepository
            .GetByIdAsync(id, trackChanges: true);

        if (activity is null)
        {
            throw new ResourceNotFoundException(
                $"Activity with id {id} not found.");
        }

        var module = await _uow.ModuleRepository
            .GetByIdAsync(activity.ModuleId);

        if (module is null)
        {
            throw new ResourceNotFoundException(
                $"Module with id {activity.ModuleId} not found.");
        }

        ValidateTimes(dto.StartTime, dto.EndTime);

        ValidateModulePeriod(
            dto.StartTime,
            dto.EndTime,
            module.StartDate,
            module.EndDate);

        var type = ParseActivityType(dto.Type);

        await ValidateNoOverlapAsync(
            activity.ModuleId,
            dto.StartTime,
            dto.EndTime,
            id);

        activity.Type = type;
        activity.Name = dto.Name;
        activity.Description = dto.Description;
        activity.StartTime = dto.StartTime;
        activity.EndTime = dto.EndTime;

        _uow.ActivityRepository.Update(activity);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var activity = await _uow.ActivityRepository
            .GetByIdAsync(id, trackChanges: true);

        if (activity is null)
        {
            throw new ResourceNotFoundException(
                $"Activity with id {id} not found.");
        }

        _uow.ActivityRepository.Delete(activity);
        await _uow.SaveChangesAsync();
    }

    private static void ValidateTimes(
        DateTime startTime,
        DateTime endTime)
    {
        if (startTime >= endTime)
        {
            throw new ArgumentException(
                "Activity start time must be earlier than end time.");
        }
    }



    private static void ValidateModulePeriod(
        DateTime startTime,
        DateTime endTime,
        DateTime moduleStartDate,
        DateTime moduleEndDate)
    {
        var moduleStart = moduleStartDate.Date;
        var moduleEndExclusive = moduleEndDate.Date.AddDays(1);

        if (startTime < moduleStart ||
            endTime > moduleEndExclusive)
        {
            throw new ArgumentException(
                "Activity must be within the module dates and times.");
        }
    }



    private async Task ValidateNoOverlapAsync(
        int moduleId,
        DateTime startTime,
        DateTime endTime,
        int? excludeActivityId = null)
    {
        var activities = await _uow.ActivityRepository
            .GetByModuleIdAsync(moduleId);

        var overlaps = activities.Any(existing =>
            existing.Id != excludeActivityId &&
            startTime < existing.EndTime &&
            endTime > existing.StartTime);

        if (overlaps)
        {
            throw new ArgumentException(
                "Activity times cannot overlap with another activity in the same module.");
        }
    }

    public async Task<PagedList<ActivityDto>> GetPagedAsync(
    PageParameters pageParameters,
    int? courseId = null)
    {
        var paged = await _uow.ActivityRepository
            .GetPagedAsync(pageParameters, courseId);

        return new PagedList<ActivityDto>(
            paged.Items.Select(a => new ActivityDto(
                a.Id, a.ModuleId, a.Type.ToString(),
                a.Name, a.Description, a.StartTime, a.EndTime)).ToList(),
            paged.PageNumber, paged.PageSize,
            paged.TotalCount, paged.TotalPages);
    }

    private static ActivityType ParseActivityType(string type)
    {
        if (!Enum.TryParse<ActivityType>(
                type,
                ignoreCase: false,
                out var activityType) ||
            !Enum.IsDefined(typeof(ActivityType), activityType))
        {
            throw new ArgumentException(
                "Activity type must be Lecture, ELearning, Exercise, Assignment or Other.");
        }

        return activityType;
    }

    private static ActivityDto MapToDto(Activity activity)
    {
        return new ActivityDto(
            activity.Id,
            activity.ModuleId,
            activity.Type.ToString(),
            activity.Name,
            activity.Description,
            activity.StartTime,
            activity.EndTime);
    }
}
