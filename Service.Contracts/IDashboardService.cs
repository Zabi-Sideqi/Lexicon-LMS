using LMS.Shared.DTOs.DashboardDtos;
namespace Service.Contracts
{
    public interface IDashboardService
    {
        Task<TeacherDashboardDto> GetTeacherDashboardDtoAsync(string TeacherId);
    }
}
