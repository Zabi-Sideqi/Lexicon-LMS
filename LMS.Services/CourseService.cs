using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;

namespace LMS.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public CourseService(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task<IEnumerable<CourseDto>> GetAllAsync()
    {
        var courses = await _uow.CourseRepository.GetAllAsync();

        return courses.Select(c => new CourseDto(
            c.Id,
            c.Name,
            c.Description,
            c.StartDate,
            c.EndDate));
    }

    public async Task<CourseDto?> GetByIdAsync(int id)
    {
        var course = await _uow.CourseRepository.GetByIdAsync(id);

        if (course is null)
            return null;

        return new CourseDto(
            course.Id,
            course.Name,
            course.Description,
            course.StartDate,
            course.EndDate);
    }

    public async Task<CourseDto> CreateAsync(CourseForCreationDto dto)
    {
        if (dto.StartDate > dto.EndDate)
            throw new ArgumentException("Start date cannot be later than end date.");

        var course = new Course
        {
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        _uow.CourseRepository.Create(course);
        await _uow.SaveChangesAsync();

        return new CourseDto(
            course.Id,
            course.Name,
            course.Description,
            course.StartDate,
            course.EndDate);
    }

    public async Task UpdateAsync(int id, CourseForUpdateDto dto)
    {
        if (dto.StartDate > dto.EndDate)
            throw new ArgumentException("Start date cannot be later than end date.");

        var course = await _uow.CourseRepository.GetByIdAsync(id, trackChanges: true);

        if (course is null)
            throw new KeyNotFoundException($"Course with id {id} not found.");

        course.Name = dto.Name;
        course.Description = dto.Description;
        course.StartDate = dto.StartDate;
        course.EndDate = dto.EndDate;

        await _uow.SaveChangesAsync();
    }

    public async Task<StudentCourseDto?> GetMyCourseAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user?.CourseId is null)
            return null;

        var course = await _uow.CourseRepository.GetByIdAsync(user.CourseId.Value);

        if (course is null)
            return null;

        return new StudentCourseDto(
            course.Id,
            course.Name,
            course.Description,
            course.StartDate,
            course.EndDate);
    }

    public async Task<IEnumerable<CourseParticipantDto>> GetCourseParticipantsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user?.CourseId is null)
            return [];

        var course = await _uow.CourseRepository.GetCourseWithStudentsAsync(user.CourseId.Value);

        if (course is null)
            return [];

        return course.Students.Select(s => new CourseParticipantDto(
            s.Id,
            s.UserName ?? "Unknown",
            s.Email ?? string.Empty));
    }
}