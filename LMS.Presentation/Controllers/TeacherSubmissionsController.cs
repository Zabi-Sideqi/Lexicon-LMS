
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.CommonDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/teacher/submissions")]
[ApiController]
[Authorize(Policy = "Teacher")]
[Produces("application/json")]
public class TeacherSubmissionsController(
    IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeacherSubmissionDto>>>
        GetSubmissions()
    {
        var teacherId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            var submissions = await _serviceManager.SubmissionService
                .GetSubmissionsForTeacherAsync(teacherId);

            return Ok(submissions);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedList<TeacherSubmissionDto>>> GetPaged(
    [FromQuery] PageParameters pageParameters,
    [FromQuery] bool? hasFeedback = null)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            var paged = await _serviceManager.SubmissionService
                .GetSubmissionsForTeacherPagedAsync(teacherId, pageParameters, hasFeedback);

            return Ok(paged);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
