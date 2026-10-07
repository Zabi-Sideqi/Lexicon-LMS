namespace Domain.Contracts;

public interface IUnitOfWork
{
    ICourseRepository CourseRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}