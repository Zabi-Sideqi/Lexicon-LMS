using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.CourseDtos;
using Service.Contracts;

namespace LMS.Services
{
    public class CourseService: ICourseService
    {
        private readonly IUnitOfWork _uow;

        public CourseService(IUnitOfWork uow)
        {
            _uow = uow;
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
    }
}
