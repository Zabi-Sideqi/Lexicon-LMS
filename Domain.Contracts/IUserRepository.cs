using Domain.Models.Entities;
using LMS.Shared.DTOs.CommonDtos;

namespace Domain.Contracts;

public interface IUserRepository
{
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<PagedList<ApplicationUser>> GetPagedAsync(PageParameters pageParameters);
    Task AddUserAsync(ApplicationUser user);
    Task UpdateUserAsync(ApplicationUser user);
    Task DeleteUserAsync(ApplicationUser user);
}