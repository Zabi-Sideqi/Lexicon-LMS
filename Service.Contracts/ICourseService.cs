using LMS.Shared.DTOs.CourseDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetAllAsync();
        Task<CourseDto?> GetByIdAsync(int id);
        Task<CourseDto> CreateAsync(CourseForCreationDto dto);
        Task UpdateAsync(int id, CourseForUpdateDto dto);
    }
}
