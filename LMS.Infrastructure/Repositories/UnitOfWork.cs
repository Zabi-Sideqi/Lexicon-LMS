
using Domain.Contracts;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Lazy<ICourseRepository> _courseRepository;
    private IUserRepository? _userRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _courseRepository = new Lazy<ICourseRepository>(
            () => new CourseRepository(context));
    }

    public ICourseRepository CourseRepository =>
        _courseRepository.Value;

    public IUserRepository UserRepository =>
        _userRepository ??= new UserRepository(_context);

    public async Task<bool> CourseExistsAsync(int courseId)
    {
        return await _context.Courses
            .AnyAsync(course => course.Id == courseId);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
