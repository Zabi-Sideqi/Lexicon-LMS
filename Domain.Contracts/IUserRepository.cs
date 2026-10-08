using Domain.Models.Entities;
namespace Domain.Contracts;

public interface IUserRepository
{
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task AddUserAsync(ApplicationUser user);
    Task UpdateUserAsync(ApplicationUser user);
    Task DeleteUserAsync(ApplicationUser user);
}