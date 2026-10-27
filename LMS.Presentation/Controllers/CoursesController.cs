using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/courses")]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Authorize]
public class CoursesController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(
        Summary = "Get all courses",
        Description = "Returns a list of all courses.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Courses retrieved", typeof(IEnumerable<CourseDto>))]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetAll()
    {
        var courses = await _serviceManager.CourseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(
        Summary = "Get a course",
        Description = "Returns a course by id.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Course found", typeof(CourseDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Course not found")]
    public async Task<ActionResult<CourseDto>> GetById(int id)
    {
        var course = await _serviceManager.CourseService.GetByIdAsync(id);

        if (course is null)
            return NotFound();

        return Ok(course);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(
        Summary = "Create a course",
        Description = "Creates a new course. Start date must not be after end date.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Course created", typeof(CourseDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data")]

    public async Task<ActionResult<CourseDto>> Create(CourseForCreationDto dto)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        var created = await _serviceManager.CourseService
            .CreateAsync(dto, teacherId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(
        Summary = "Update a course",
        Description = "Updates an existing course.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Course updated")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Course not found")]
    public async Task<IActionResult> Update(int id, CourseForUpdateDto dto)
    {
        await _serviceManager.CourseService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    [SwaggerOperation(
        Summary = "Get the current student's course",
        Description = "Returns the course assigned to the authenticated student.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Course found", typeof(StudentCourseDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Student has no assigned course")]
    public async Task<ActionResult<StudentCourseDto>> GetMyCourse()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var course = await _serviceManager.CourseService.GetMyCourseAsync(userId);

        if (course is null)
            return NotFound();

        return Ok(course);
    }

    [HttpGet("my/participants")]
    [Authorize(Roles = "Student")]
    [SwaggerOperation(
        Summary = "Get participants of the current student's course",
        Description = "Returns a list of participants in the same course as the authenticated student.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Participants retrieved", typeof(IEnumerable<CourseParticipantDto>))]
    public async Task<ActionResult<IEnumerable<CourseParticipantDto>>> GetParticipants()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var participants = await _serviceManager.CourseService.GetCourseParticipantsAsync(userId);

        return Ok(participants);
    }
}