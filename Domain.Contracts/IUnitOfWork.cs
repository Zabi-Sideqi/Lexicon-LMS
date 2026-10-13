namespace Domain.Contracts;

public interface IUnitOfWork
{
    ICourseRepository CourseRepository { get; }
    IModuleRepository ModuleRepository { get; }
    IActivityRepository ActivityRepository { get; }
    IUserRepository UserRepository { get; }

    Task<bool> CourseExistsAsync(int courseId);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}