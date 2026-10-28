using LMS.Shared.DTOs.AssignmentDtos;

namespace Service.Contracts;

public interface ISubmissionService
{
    Task<IEnumerable<StudentAssignmentDto>> GetMyAssignmentsAsync(
        string userId);

    Task<StudentAssignmentDto?> GetMyAssignmentAsync(
        string userId,
        int assignmentId);

    Task<StudentAssignmentDto> SubmitAsync(
        string userId,
        int assignmentId,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize);

    Task<IEnumerable<TeacherSubmissionDto>> GetSubmissionsForTeacherAsync(
        string teacherId,
        TeacherSubmissionFilterDto? filter = null);
}