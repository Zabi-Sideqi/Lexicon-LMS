
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ModuleDtos;
using Service.Contracts;


namespace LMS.Services;

public class ModuleService : IModuleService
{
    private readonly IUnitOfWork _uow;

    public ModuleService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<ModuleDto>> GetAllAsync()
    {
        var modules = await _uow.ModuleRepository.GetAllAsync();

        return modules.Select(MapToDto);
    }

    public async Task<ModuleDto?> GetByIdAsync(int id)
    {
        var module = await _uow.ModuleRepository.GetByIdAsync(id);

        return module is null ? null : MapToDto(module);
    }

    public async Task<IEnumerable<ModuleDto>> GetByCourseIdAsync(
        int courseId)
    {
        var course = await _uow.CourseRepository.GetByIdAsync(courseId);

        if (course is null)
            throw new ResourceNotFoundException(
                $"Course with id {courseId} not found.");

        var modules = await _uow.ModuleRepository
            .GetByCourseIdAsync(courseId);

        return modules
            .OrderBy(module => module.StartDate)
            .Select(MapToDto);
    }

    public async Task<ModuleDto> CreateAsync(
        ModuleForCreationDto dto)
    {
        var course = await _uow.CourseRepository
            .GetByIdAsync(dto.CourseId);

        if (course is null)
            throw new ResourceNotFoundException(
                $"Course with id {dto.CourseId} not found.");

        ValidateDates(
            dto.StartDate,
            dto.EndDate,
            course.StartDate,
            course.EndDate);

        await ValidateNoOverlapAsync(
            dto.CourseId,
            dto.StartDate,
            dto.EndDate);

        var module = new Module
        {
            CourseId = dto.CourseId,
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        _uow.ModuleRepository.Create(module);
        await _uow.SaveChangesAsync();

        return MapToDto(module);
    }

    public async Task UpdateAsync(
        int id,
        ModuleForUpdateDto dto)
    {
        var module = await _uow.ModuleRepository
            .GetByIdAsync(id, trackChanges: true);

        if (module is null)
            throw new ResourceNotFoundException(
                $"Module with id {id} not found.");

        var course = await _uow.CourseRepository
            .GetByIdAsync(dto.CourseId);

        if (course is null)
            throw new ResourceNotFoundException(
                $"Course with id {dto.CourseId} not found.");

        ValidateDates(
            dto.StartDate,
            dto.EndDate,
            course.StartDate,
            course.EndDate);

        await ValidateNoOverlapAsync(
            dto.CourseId,
            dto.StartDate,
            dto.EndDate,
            id);

        module.CourseId = dto.CourseId;
        module.Name = dto.Name;
        module.Description = dto.Description;
        module.StartDate = dto.StartDate;
        module.EndDate = dto.EndDate;

        _uow.ModuleRepository.Update(module);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var module = await _uow.ModuleRepository
            .GetByIdAsync(id, trackChanges: true);

        if (module is null)
            throw new ResourceNotFoundException(
                $"Module with id {id} not found.");

        _uow.ModuleRepository.Delete(module);
        await _uow.SaveChangesAsync();
    }

    private static void ValidateDates(
        DateTime startDate,
        DateTime endDate,
        DateTime courseStartDate,
        DateTime courseEndDate)
    {
        if (startDate > endDate)
            throw new ArgumentException(
                "Module start date cannot be later than end date.");

        if (startDate < courseStartDate ||
            endDate > courseEndDate)
        {
            throw new ArgumentException(
                "Module dates must be within the course dates.");
        }
    }

    private async Task ValidateNoOverlapAsync(
        int courseId,
        DateTime startDate,
        DateTime endDate,
        int? excludeModuleId = null)
    {
        var modules = await _uow.ModuleRepository
            .GetByCourseIdAsync(courseId);

        var overlaps = modules.Any(existing =>
            existing.Id != excludeModuleId &&
            startDate <= existing.EndDate &&
            endDate >= existing.StartDate);

        if (overlaps)
            throw new ArgumentException(
                "Module dates cannot overlap with another module in the same course.");
    }

    private static ModuleDto MapToDto(Module module)
    {
        return new ModuleDto(
            module.Id,
            module.CourseId,
            module.Name,
            module.Description,
            module.StartDate,
            module.EndDate);
    }
}

