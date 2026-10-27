using LMS.Shared.DTOs.DashboardDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/dashboard")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class DashboardController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard()
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        try
        {
            var dashboard = await _serviceManager.DashboardService
                .GetStudentDashboardAsync(studentId);

            return Ok(dashboard);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}