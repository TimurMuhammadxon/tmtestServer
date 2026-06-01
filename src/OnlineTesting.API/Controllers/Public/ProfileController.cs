using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Auth.Commands.Login;
using OnlineTesting.Application.Users.Commands.SetCredentials;
using OnlineTesting.Application.Users.Commands.UpdateProfile;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("users/me")]
[Authorize]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly ISender _mediator;

    public ProfileController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPatch]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateProfileResponse>> UpdateProfile(
        [FromBody] UpdateProfileCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("credentials")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> SetCredentials(
        [FromBody] SetCredentialsCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
