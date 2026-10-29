
using Domain.Models.Entities;
using LMS.Shared.DTOs.ActivityDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize]
public class ActivitiesController(
    IServiceManager serviceManager,
    UserManager<ApplicationUser> userManager,
    IAuthorizationService authorizationService) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    // Lärare kan se alla aktiviteter.
    // Studenter kan bara se aktiviteter från sin egen kurs.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetAll()
    {
        if (!User.IsInRole("Teacher") &&
            !User.IsInRole("Student"))
        {
            return Forbid();
        }

        var activities = await _serviceManager.ActivityService.GetAllAsync();

        if (User.IsInRole("Teacher"))
            return Ok(activities);

        var user = await _userManager.GetUserAsync(User);

        if (user?.CourseId is not int courseId)
            return Forbid();

        var modules = await _serviceManager.ModuleService.GetAllAsync();

        var allowedModuleIds = modules
            .Where(module => module.CourseId == courseId)
            .Select(module => module.Id)
            .ToHashSet();

        var filteredActivities = activities
            .Where(activity => allowedModuleIds.Contains(activity.ModuleId))
            .OrderBy(activity => activity.StartTime);

        return Ok(filteredActivities);
    }

    // Hämtar en aktivitet endast om användaren har rätt kursåtkomst.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ActivityDto>> GetById(int id)
    {
        var activity = await _serviceManager.ActivityService.GetByIdAsync(id);

        if (activity is null)
            return NotFound();

        if (await CanReadModuleAsync(activity.ModuleId))
            return Ok(activity);

        return Forbid();
    }


    // Hämtar aktiviteter för en modul endast om användaren har åtkomst.
    [HttpGet("module/{moduleId:int}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByModuleId(
        int moduleId)
    {
        // Endast lärare och studenter får läsa aktiviteter.
        if (!User.IsInRole("Teacher") &&
            !User.IsInRole("Student"))
        {
            return Forbid();
        }

        var module = await _serviceManager.ModuleService.GetByIdAsync(moduleId);

        if (module is null)
            return NotFound();

        // Studenter får endast läsa aktiviteter från sin egen kurs.
        if (User.IsInRole("Student"))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                module.CourseId,
                "SameCourse");

            if (!authorizationResult.Succeeded)
                return Forbid();
        }

        var activities =
            await _serviceManager.ActivityService.GetByModuleIdAsync(moduleId);

        return Ok(activities.OrderBy(activity => activity.StartTime));
    }


    // Skapar en aktivitet. Endast lärare får göra detta.
    [Authorize(Policy = "Teacher")]
    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(
        [FromBody] ActivityCreateDto dto)
    {
        var created = await _serviceManager.ActivityService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    // Uppdaterar en aktivitet. Endast lärare får göra detta.
    [Authorize(Policy = "Teacher")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(
        int id,
        [FromBody] ActivityUpdateDto dto)
    {
        await _serviceManager.ActivityService.UpdateAsync(id, dto);
        return NoContent();
    }

    // Tar bort en aktivitet. Endast lärare får göra detta.
    [Authorize(Policy = "Teacher")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _serviceManager.ActivityService.DeleteAsync(id);
        return NoContent();
    }


    private async Task<bool> CanReadModuleAsync(int moduleId)
    {
        if (User.IsInRole("Teacher"))
            return true;

        if (!User.IsInRole("Student"))
            return false;

        var module = await _serviceManager.ModuleService.GetByIdAsync(moduleId);

        if (module is null)
            return false;

        var result = await _authorizationService.AuthorizeAsync(
            User,
            module.CourseId,
            "SameCourse");

        return result.Succeeded;
    }

}
