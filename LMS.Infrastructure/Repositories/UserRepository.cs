using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }
    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _context.Users.FindAsync(userId);
    }
    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    public async Task AddUserAsync(ApplicationUser user)
    {
        await _context.Users.AddAsync(user);
    }
    public async Task UpdateUserAsync(ApplicationUser user)
    {
        _context.Users.Update(user);
    }
    public async Task DeleteUserAsync(ApplicationUser user)
    {
        _context.Users.Remove(user);
    } 

}