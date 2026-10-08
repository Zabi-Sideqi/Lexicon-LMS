

namespace LMS.Shared.DTOs.UserDtos
{
    public record UserResponseDto
    {
        public string Id { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public int? CourseId { get; init; }
    }
}
