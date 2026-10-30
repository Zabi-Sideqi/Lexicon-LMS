using LMS.Shared.DTOs.CommonDtos;
using Domain.Models.Entities;

namespace Domain.Contracts;

public interface ISubmissionRepository
{
    Task<IEnumerable<Submission>> GetByStudentIdAsync(
        string studentId,
        bool trackChanges = false);

    Task<Submission?> GetByIdAsync(
        int submissionId,
        bool trackChanges = false);
    Task<IEnumerable<Submission>> GetByCourseIdAsync(
        int courseId,
        bool trackChanges = false);

    Task<PagedList<Submission>> GetPagedAsync(
        PageParameters pageParameters,
        bool? hasFeedback = null);

    void Create(Submission submission);
}
