
using Domain.Contracts;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IUserRepository? _userRepository;
    private ICourseRepository? _courseRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository UserRepository =>
        _userRepository ??= new UserRepository(_context);

    public ICourseRepository CourseRepository =>
        _courseRepository ??= new CourseRepository(_context);

    public async Task<bool> CourseExistsAsync(int courseId)
    {
        return await _context.Courses
            .AnyAsync(course => course.Id == courseId);
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
