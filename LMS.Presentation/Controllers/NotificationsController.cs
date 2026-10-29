
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/notifications")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = "Student")]
public class NotificationsController(IServiceManager serviceManager)
    : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var studentId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(studentId))
        {
            return Unauthorized();
        }

        var notifications = await _serviceManager.NotificationService
            .GetByStudentIdAsync(studentId);

        return Ok(notifications);
    }
}
