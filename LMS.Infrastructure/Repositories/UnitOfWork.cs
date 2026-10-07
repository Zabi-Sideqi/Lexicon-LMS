using Domain.Contracts;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Lazy<ICourseRepository> _courseRepository;

    public ICourseRepository CourseRepository => _courseRepository.Value;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _courseRepository = new Lazy<ICourseRepository>(
            () => new CourseRepository(context));
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
