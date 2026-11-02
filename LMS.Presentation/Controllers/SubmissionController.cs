using System.Security.Claims;
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Route("api/submissions")]
[ApiController]
[Authorize(Policy = "Teacher")]
public class SubmissionController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public SubmissionController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpGet("teacher")]
    public async Task<ActionResult<IEnumerable<TeacherSubmissionDto>>>
        GetTeacherSubmissions(
            [FromQuery] TeacherSubmissionFilterDto? filter)
    {
        var teacherId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        var submissions = await _serviceManager
            .SubmissionService
            .GetSubmissionsForTeacherAsync(teacherId, filter);

        return Ok(submissions);
    }

    [HttpGet("{submissionId:int}/download")]
    public async Task<IActionResult> DownloadSubmission(int submissionId)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            var (stream, fileName, contentType) = await _serviceManager
                .SubmissionService
                .DownloadSubmissionAsync(teacherId, submissionId);

            return File(stream, contentType, fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}