using LMS.Presentation.DTOs;
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/assignments")]
[ApiController]
[Produces("application/json")]
[Authorize(Roles = "Student")]
public class AssignmentsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet("my")]
    [SwaggerOperation(
        Summary = "Get the current student's assignments",
        Description = "Returns the assignments in the authenticated student's course, with deadline and status.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignments retrieved", typeof(IEnumerable<StudentAssignmentDto>))]
    public async Task<ActionResult<IEnumerable<StudentAssignmentDto>>> GetMyAssignments()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var assignments = await _serviceManager.SubmissionService.GetMyAssignmentsAsync(userId);

        return Ok(assignments);
    }

    [HttpGet("my/{id:int}")]
    [SwaggerOperation(
        Summary = "Get one of the current student's assignments",
        Description = "Returns an assignment in the authenticated student's course.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignment found", typeof(StudentAssignmentDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Assignment not found in the student's course")]
    public async Task<ActionResult<StudentAssignmentDto>> GetMyAssignment(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var assignment = await _serviceManager.SubmissionService.GetMyAssignmentAsync(userId, id);

        if (assignment is null)
            return NotFound();

        return Ok(assignment);
    }

    [HttpPost("my/{id:int}/submission")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Hand in a file to an assignment",
        Description = "Uploads a file as the authenticated student's submission. The submission time is set by the server.")]
    [SwaggerResponse(StatusCodes.Status200OK, "File handed in. Returns the assignment with its new status", typeof(StudentAssignmentDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid file")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Assignment not found in the student's course")]
    public async Task<ActionResult<StudentAssignmentDto>> Submit(
        int id,
        [FromForm] SubmissionUploadDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        await using var stream = dto.File.OpenReadStream();

        var assignment = await _serviceManager.SubmissionService.SubmitAsync(
            userId,
            id,
            stream,
            dto.File.FileName,
            dto.File.ContentType,
            dto.File.Length);

        return Ok(assignment);
    }
}
