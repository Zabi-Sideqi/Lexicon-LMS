
using LMS.Shared.DTOs.FeedbackDtos;

namespace Service.Contracts;

public interface IFeedbackService
{
    Task<IEnumerable<FeedbackDto>> GetBySubmissionAsync(
        int submissionId,
        string studentId);

    Task<FeedbackDto> CreateAsync(
        int submissionId,
        string teacherId,
        FeedbackForCreationDto dto);
}
