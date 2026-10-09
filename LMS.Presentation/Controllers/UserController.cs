
﻿using LMS.Shared.DTOs.UserDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Route("api/users")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class UserController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public UserController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
    {
        var users = await _serviceManager.UserService.GetAllUsersAsync();

        return Ok(users);
    }

    // GET: api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(string id)
    {
        try
        {
            var user = await _serviceManager.UserService.GetUserByIdAsync(id);

            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("User not found.");
        }
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> CreateUser(
       [FromBody] UserCreateDto userDto)
    {
        try
        {
            var createdUser =
                await _serviceManager.UserService.CreateUserAsync(userDto);

            return Ok(createdUser);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // PUT: api/users/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(
    string id,
    [FromBody] UserUpdateDto userDto)
    {
        try
        {
            var updatedUser =
                await _serviceManager.UserService.UpdateUserAsync(id, userDto);

            return Ok(updatedUser);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("User not found.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var deleted = await _serviceManager.UserService.DeleteUserAsync(id);
            if (!deleted)
            {
                return NotFound("User not found.");
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}

