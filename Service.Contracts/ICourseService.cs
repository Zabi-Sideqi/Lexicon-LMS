using LMS.Shared.DTOs.CourseDtos;

namespace Service.Contracts;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllAsync();
    Task<CourseDto?> GetByIdAsync(int id);
    Task<CourseDto> CreateAsync(CourseForCreationDto dto);
    Task UpdateAsync(int id, CourseForUpdateDto dto);

    Task<StudentCourseDto?> GetMyCourseAsync(string userId);
    Task<IEnumerable<CourseParticipantDto>> GetCourseParticipantsAsync(string userId);
}