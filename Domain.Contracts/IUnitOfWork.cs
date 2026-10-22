namespace Domain.Contracts;

public interface IUnitOfWork
{
    ICourseRepository CourseRepository { get; }
    IModuleRepository ModuleRepository { get; }
    IActivityRepository ActivityRepository { get; }
    IUserRepository UserRepository { get; }
    IDocumentRepository DocumentRepository { get; }
    ISubmissionRepository SubmissionRepository { get; }
    IFeedbackRepository FeedbackRepository { get; }
    INotificationRepository NotificationRepository { get; }


    Task<bool> CourseExistsAsync(int courseId);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}