using Domain.Models.Entities;


namespace Domain.Contracts
{
    public interface IFeedbackRepository
    {
        Task<IEnumerable<Feedback>> GetBySubmissionIdAsync(
            int submissionId,
            bool trackChanges = false);
        void Create(Feedback feedback);
    }
}
