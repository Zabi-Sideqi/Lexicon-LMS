
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/teacher/submissions")]
[ApiController]
[Authorize(Roles = "Teacher")]
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
}
