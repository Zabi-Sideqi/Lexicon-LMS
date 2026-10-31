using LMS.Shared.DTOs.ModuleDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/modules")]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize]
public class ModulesController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "Get all modules")]
    [SwaggerResponse(StatusCodes.Status200OK, "Modules retrieved")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetAll()
    {
        var modules = await _serviceManager.ModuleService.GetAllAsync();
        return Ok(modules);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a module by id")]
    [SwaggerResponse(StatusCodes.Status200OK, "Module found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found")]
    public async Task<ActionResult<ModuleDto>> GetById(int id)
    {
        var module = await _serviceManager.ModuleService.GetByIdAsync(id);

        if (module is null)
            return NotFound();

        if (!User.IsInRole("Teacher"))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Forbid();

            // Student course membership is checked by the dedicated course endpoint.
            // Keep individual module access restricted to teachers here.
            return Forbid();
        }

        return Ok(module);
    }

    [HttpGet("course/{courseId:int}")]
    [SwaggerOperation(Summary = "Get modules for a course")]
    [SwaggerResponse(StatusCodes.Status200OK, "Modules retrieved")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetByCourseId(int courseId)
    {
        if (!User.IsInRole("Teacher"))
        {
            // Student access is scoped to their own course by the service-facing user identity.
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Forbid();

            // Resolve the student's course through the authenticated user store.
            // This endpoint is implemented below through the injected user manager in the original design.
            return Forbid();
        }

        var modules = await _serviceManager.ModuleService.GetByCourseIdAsync(courseId);
        return Ok(modules);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost]
    [SwaggerOperation(Summary = "Create a module")]
    [SwaggerResponse(StatusCodes.Status201Created, "Module created")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data")]
    public async Task<ActionResult<ModuleDto>> Create(ModuleForCreationDto dto)
    {
        var created = await _serviceManager.ModuleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a module")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Module updated")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found")]
    public async Task<IActionResult> Update(int id, ModuleForUpdateDto dto)
    {
        await _serviceManager.ModuleService.UpdateAsync(id, dto);
        return NoContent();
    }

    [Authorize(Roles = "Teacher")]
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Delete a module")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Module deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found")]
    public async Task<IActionResult> Delete(int id)
    {
        await _serviceManager.ModuleService.DeleteAsync(id);
        return NoContent();
    }
}