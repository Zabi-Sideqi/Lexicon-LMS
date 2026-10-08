using LMS.Shared.DTOs.UserDtos;

namespace Service.Contracts;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto> GetUserByIdAsync(string id);
    Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto);
    Task<UserResponseDto> UpdateUserAsync(string id, UserUpdateDto userDto);
    Task<bool> DeleteUserAsync(string id);
}