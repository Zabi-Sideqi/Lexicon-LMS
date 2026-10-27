
using LMS.Shared.DTOs.DashboardDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/dashboard")]
[ApiController]
[Authorize(Roles = "Teacher")]
[Produces("application/json")]
public class DashboardController(IServiceManager serviceManager)
    : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet("teacher")]
    public async Task<ActionResult<TeacherDashboardDto>>
        GetTeacherDashboard()
    {
        var teacherId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            var dashboard = await _serviceManager.DashboardService
                .GetTeacherDashboardDtoAsync(teacherId);

            return Ok(dashboard);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
