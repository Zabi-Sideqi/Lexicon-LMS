using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LMS.Infrastructure.Extensions;
using LMS.Shared.DTOs.CommonDtos;

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

    public async Task<PagedList<ApplicationUser>> GetPagedAsync(PageParameters pageParameters)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pageParameters.SearchTerm))
        {
            var search = pageParameters.SearchTerm;
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                (u.UserName != null && u.UserName.Contains(search)));
        }

        query = pageParameters.SortOrder switch
        {
            "email" => query.OrderBy(u => u.Email),
            "email_desc" => query.OrderByDescending(u => u.Email),
            "username" => query.OrderBy(u => u.UserName),
            "username_desc" => query.OrderByDescending(u => u.UserName),
            _ => query.OrderBy(u => u.UserName)
        };

        return await query.ToPagedListAsync(pageParameters);
    }

}