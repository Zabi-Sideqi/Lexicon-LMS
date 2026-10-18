using Domain.Models.Entities;

namespace Domain.Contracts;

public interface IDocumentRepository
{
    Task<IEnumerable<Document>> GetByCourseIdAsync(int courseId, bool trackChanges = false);
    Task<IEnumerable<Document>> GetByModuleIdAsync(int moduleId, bool trackChanges = false);
    Task<Document?> GetByIdAsync(int id, bool trackChanges = false);
    void Create(Document document);
    void Delete(Document document);
}