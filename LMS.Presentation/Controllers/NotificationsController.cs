
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/notifications")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = "StudentOrTeacher")]
public class NotificationsController(IServiceManager serviceManager)
    : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var recipientId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return Unauthorized();
        }

        var notifications = await _serviceManager.NotificationService
            .GetByRecipientIdAsync(recipientId);

        return Ok(notifications);
    }

    [HttpPatch("{notificationId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var recipientId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return Unauthorized();
        }

        var updated = await _serviceManager.NotificationService
            .MarkAsReadForRecipientAsync(notificationId, recipientId);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }
}
