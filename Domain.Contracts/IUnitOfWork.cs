namespace Domain.Contracts;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    Task<bool> CourseExistsAsync(int courseId);
    Task<int> SaveChangesAsync();

}