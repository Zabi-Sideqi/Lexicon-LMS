using LMS.Shared.DTOs.AssignmentDtos;

namespace Service.Contracts;

public interface ISubmissionService
{
    Task<IEnumerable<StudentAssignmentDto>> GetMyAssignmentsAsync(string userId);

    Task<StudentAssignmentDto?> GetMyAssignmentAsync(string userId, int assignmentId);
}
