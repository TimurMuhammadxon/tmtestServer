using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Auth.Commands.Login;
using OnlineTesting.Application.Auth.Commands.Logout;
using OnlineTesting.Application.Auth.Commands.Refresh;
using OnlineTesting.Application.Auth.Commands.Register;

namespace OnlineTesting.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    public record RefreshRequest(string RefreshToken);
    public record LogoutRequest(string RefreshToken);

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Register), new { id = result.Id }, result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshCommand(body.RefreshToken), ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest body, CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(body.RefreshToken), ct);
        return NoContent();
    }
}