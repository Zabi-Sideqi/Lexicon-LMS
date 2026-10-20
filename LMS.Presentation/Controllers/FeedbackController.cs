
using LMS.Shared.DTOs.FeedbackDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/feedback")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class FeedbackController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet("submission/{submissionId:int}")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetBySubmission(
        int submissionId)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        try
        {
            var feedback = await _serviceManager.FeedbackService
                .GetBySubmissionAsync(submissionId, studentId);

            return Ok(feedback);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("submission/{submissionId:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<FeedbackDto>> Create(
        int submissionId,
        [FromBody] FeedbackForCreationDto dto)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            var feedback = await _serviceManager.FeedbackService
                .CreateAsync(submissionId, teacherId, dto);

            return Ok(feedback);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
