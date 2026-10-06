using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Presentation.Controllers;

[Route("api/users")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class UserController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok("User endpoint is working.");
    }
}