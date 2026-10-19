
using Domain.Models.Entities;

namespace Domain.Contracts;

public interface ISubmissionRepository
{
    Task<IEnumerable<Submission>> GetByStudentIdAsync(
        string studentId,
        bool trackChanges = false);

    void Create(Submission submission);
}
