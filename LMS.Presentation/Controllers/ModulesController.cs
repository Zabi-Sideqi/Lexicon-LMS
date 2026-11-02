
using Domain.Models.Entities;
using LMS.Shared.DTOs.ModuleDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/modules")]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize]
public class ModulesController(
    IServiceManager serviceManager,
    UserManager<ApplicationUser> userManager,
        IAuthorizationService authorizationService) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    private bool IsTeacher =>
        User.IsInRole("Teacher");

    private async Task<int?> GetStudentCourseIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return null;

        var user = await _userManager.FindByIdAsync(userId);

        return user?.CourseId;
    }

    [HttpGet]
    [Authorize(Roles = "Teacher,Student")]
    [SwaggerOperation(Summary = "Get all modules")]
    [SwaggerResponse(StatusCodes.Status200OK, "Modules retrieved")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetAll()
    {
        if (IsTeacher)
        {
            var allModules =
                await _serviceManager.ModuleService.GetAllAsync();

            return Ok(allModules);
        }

        var courseId = await GetStudentCourseIdAsync();

        if (courseId is null)
            return Forbid();

        var studentModules =
            await _serviceManager.ModuleService.GetByCourseIdAsync(courseId.Value);

        return Ok(studentModules);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a module by id")]
    [SwaggerResponse(StatusCodes.Status200OK, "Module found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<ActionResult<ModuleDto>> GetById(int id)
    {
        var module =
            await _serviceManager.ModuleService.GetByIdAsync(id);

        if (module is null)
            return NotFound();

        if (!IsTeacher)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                module.CourseId,
                "SameCourse");

            if (!authorizationResult.Succeeded)
                return Forbid();
        }

        return Ok(module);
    }

    [HttpGet("course/{courseId:int}")]
    [SwaggerOperation(Summary = "Get modules for a course")]
    [SwaggerResponse(StatusCodes.Status200OK, "Modules retrieved")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetByCourseId(
        int courseId)
    {
        if (!IsTeacher)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                courseId,
                "SameCourse");

            if (!authorizationResult.Succeeded)
                return Forbid();
        }

        var modules =
            await _serviceManager.ModuleService.GetByCourseIdAsync(courseId);

        return Ok(modules);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost]
    [SwaggerOperation(Summary = "Create a module")]
    [SwaggerResponse(StatusCodes.Status201Created, "Module created")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data")]
    public async Task<ActionResult<ModuleDto>> Create(
        ModuleForCreationDto dto)
    {
        var created =
            await _serviceManager.ModuleService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a module")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Module updated")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Module not found")]
    public async Task<IActionResult> Update(
        int id,
        ModuleForUpdateDto dto)
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
