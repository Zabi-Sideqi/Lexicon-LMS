using Domain.Models.Entities;
using LMS.Shared.DTOs.CommonDtos;

namespace Domain.Contracts
{
    public interface IActivityRepository
    {
        Task<IEnumerable<Activity>> GetAllAsync(
            bool trackChanges = false);
        Task<Activity?> GetByIdAsync(
            int id,
            bool trackChanges = false);
        Task<IEnumerable<Activity>> GetByModuleIdAsync(
            int moduleId,
            bool trackChanges = false);
        Task<IEnumerable<Activity>> GetByCourseIdAsync(
            int courseId,
            bool trackChanges = false);
        void Create(Activity activity);
        void Update(Activity activity);
        void Delete(Activity activity);
        Task<IEnumerable<Activity>> GetAssignmentsByCourseIdAsync(
            int courseId, 
            bool trackChanges = false);
        Task<PagedList<Activity>> GetPagedAsync(
            PageParameters pageParameters,
            int? courseId = null);
    }
}
