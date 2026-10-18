
using Domain.Models.Entities;

namespace Domain.Contracts;

public interface IModuleRepository
{
    Task<IEnumerable<Module>> GetAllAsync(
        bool trackChanges = false);

    Task<Module?> GetByIdAsync(
        int id,
        bool trackChanges = false);

    Task<IEnumerable<Module>> GetByCourseIdAsync(
        int courseId,
        bool trackChanges = false);

    void Create(Module module);

    void Update(Module module);

    void Delete(Module module);
}

