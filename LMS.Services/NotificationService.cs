
using Domain.Contracts;
using Domain.Models.Entities;
using Service.Contracts;

namespace LMS.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;

    public NotificationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Notification>> GetByStudentIdAsync(
        string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(studentId));
        }

        return await _uow.NotificationRepository
            .GetByStudentIdAsync(studentId);
    }
}
