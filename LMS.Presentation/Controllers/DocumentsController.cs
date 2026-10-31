using LMS.Presentation.DTOs;
using LMS.Shared.DTOs.DocumentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/documents")]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize]
public class DocumentsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpPost("upload")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
    [Authorize(Roles = "Teacher,Student")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Upload a document",
        Description = "Uploads a file and links it to a course or module.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Document uploaded")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid file or data")]
    public async Task<ActionResult<DocumentDto>> Upload(
        [FromForm] DocumentUploadDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        await using var stream = dto.File.OpenReadStream();

        var created = await _serviceManager.DocumentService.UploadAsync(
            stream,
            dto.File.FileName,
            dto.File.ContentType,
            dto.File.Length,
            dto.CourseId,
            dto.ModuleId,
            dto.ActivityId,
            userId);

        return CreatedAtAction(nameof(Download), new { id = created.Id }, created);
    }

    [HttpGet("{id:int}/download")]
    [SwaggerOperation(
        Summary = "Download a document",
        Description = "Downloads a file by id.")]
    [SwaggerResponse(StatusCodes.Status200OK, "File stream")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<IActionResult> Download(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var (stream, fileName, contentType) =
            await _serviceManager.DocumentService.DownloadAsync(id, userId);

        return File(stream, contentType, fileName);
    }

    [HttpGet("course/{courseId:int}")]
    [SwaggerOperation(
        Summary = "Get documents for a course",
        Description = "Returns all documents linked to a specific course.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Documents retrieved")]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByCourse(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var documents = await _serviceManager.DocumentService
            .GetByCourseAsync(courseId, userId);

        return Ok(documents);
    }
}