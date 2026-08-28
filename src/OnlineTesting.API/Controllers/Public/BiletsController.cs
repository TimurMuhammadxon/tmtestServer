using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Tests.Solutions.Bilets.Queries;
using OnlineTesting.Application.Tests.Solutions.Bilets.Queries.GetPublicBiletById;
using OnlineTesting.Application.Tests.Solutions.Bilets.Queries.GetPublicBilets;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("bilets")]
[AllowAnonymous]
public class BiletsController : ControllerBase
{
    private readonly IMediator _mediator;
    public BiletsController(IMediator mediator) => _mediator = mediator;

    // No caching: guests see demo-only, so any shared cache leaks that to logged-in users.
    [HttpGet]
    [ResponseCache(NoStore = true)]
    [ProducesResponseType(typeof(IReadOnlyList<PublicBiletListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPublicBiletsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ResponseCache(NoStore = true)]
    [ProducesResponseType(typeof(PublicBiletDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPublicBiletByIdQuery(id), ct);
        return Ok(result);
    }
}