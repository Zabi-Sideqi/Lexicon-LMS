using LMS.Shared.DTOs.ActivityDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Authorize]
    public class ActivitiesController(IServiceManager serviceManager)
        : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        //Hömtar alla aktiviteter
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityDto>>> GetAll()

        {
            var activities = await _serviceManager.ActivityService.GetAllAsync();
            return Ok(activities);
        }
        //Hömtar en aktivitet baserat på id
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ActivityDto>> GetById(int id)
        {
            var activity = await _serviceManager.ActivityService.GetByIdAsync(id);
            if (activity is null)
            {
                return NotFound();
            }
            return Ok(activity);
        }

        //Hämtar alla aktivitetr som tillhör en viss modul
        [HttpGet("module/{moduleId:int}")]
        public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByModuleId(int moduleId)
        {
            var activities = await _serviceManager.ActivityService.GetByModuleIdAsync(moduleId);
            return Ok(activities);

        }

    //skapar en ny aktivitet. Endast lärare får göra detta.
    [Authorize(Roles = "Teacher")]
    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(
        [FromBody] ActivityCreateDto dto)

        {
            var created = await _serviceManager.ActivityService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
            );
        }

        //Uppdaterar en aktivitet. Endast lärare får göra detta.
        [Authorize(Roles = "Teacher")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(
            int id,
            [FromBody] ActivityUpdateDto dto)
        {
            await _serviceManager.ActivityService.UpdateAsync(id, dto);
            return NoContent();
        }

        //Tar bort en aktivitet. Endast lärare får göra detta.
        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _serviceManager.ActivityService.DeleteAsync(id);
            return NoContent();
        }

    }
}
