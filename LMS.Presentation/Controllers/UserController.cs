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

    [HttpPost]
    public IActionResult CreateUser()
    {
        return Ok("User created successfully.");
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser(string id)
    {
        return Ok($"User {id} updated successfully.");
    }
}





//Zabi