namespace Domain.Contracts;

public interface IUnitOfWork
{
    ICourseRepository CourseRepository { get; }
    IUserRepository UserRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}