using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
