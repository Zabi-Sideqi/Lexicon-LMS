using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.UserDtos;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;

namespace LMS.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    public UserService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager
        )
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }
    public async Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(userDto.Email);

        if (existingUser is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = userDto.UserName,
            Email = userDto.Email,
            CourseId = userDto.CourseId
        };

        var result = await _userManager.CreateAsync(user, userDto.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, userDto.Role);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }

        return new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = userDto.Role,
            CourseId = user.CourseId
        };
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(id);

        if (user is null)
            return false;

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return true;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.UserRepository.GetAllUsersAsync();

        var result = new List<UserResponseDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            result.Add(new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty,
                CourseId = user.CourseId
            });
        }

        return result;
    }

    public async Task<UserResponseDto> GetUserByIdAsync(string id)
    {
        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(id);

        if (user is null)
            throw new KeyNotFoundException($"User with id '{id}' was not found.");

        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
            CourseId = user.CourseId
        };
    }

    public async Task<UserResponseDto> UpdateUserAsync(string id, UserUpdateDto userDto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByIdAsync(id);

        if (user is null)
            throw new KeyNotFoundException($"User with id '{id}' was not found.");
        var existingUser = await _userManager.FindByEmailAsync(userDto.Email);

        if (existingUser is not null && existingUser.Id != id)
            throw new InvalidOperationException("A user with this email already exists.");

        user.UserName = userDto.UserName;
        user.Email = userDto.Email;
        user.CourseId = userDto.CourseId;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", updateResult.Errors.Select(e => e.Description)));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (!currentRoles.Contains(userDto.Role))
        {
            if (currentRoles.Count > 0)
            {
                var removeRoleResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join("; ", removeRoleResult.Errors.Select(e => e.Description)));
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, userDto.Role);

            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", addRoleResult.Errors.Select(e => e.Description)));
            }
        }

        return new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = userDto.Role,
            CourseId = user.CourseId
        };
    }
}