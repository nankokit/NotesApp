using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.DTOs;
using NotesApp.Application.Commands;
using NotesApp.Application.Queries;
using System.Threading.Tasks;
using System;
using System.Security.Claims;

namespace NotesApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterCommand command)
    {
        var userDto = await _mediator.Send(command);

        return Ok(userDto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var token = await _mediator.Send(command);

        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetUser()
    {
        var query = new GetUserByIdQuery { Id = GetUserId() };
        var userDto = await _mediator.Send(query);

        return Ok(userDto);
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> Delete()
    {
        var command = new DeleteUserCommand { Id = GetUserId() };
        await _mediator.Send(command);

        return NoContent();
    }
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid.TryParse(userIdClaim, out var userId);
        return userId;
    }
}