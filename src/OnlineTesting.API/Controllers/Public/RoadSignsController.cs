using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Content.RoadSigns.Queries.GetRoadSigns;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("road-signs")]
[AllowAnonymous]
public class RoadSignsController : ControllerBase
{
    private readonly IMediator _mediator;
    public RoadSignsController(IMediator mediator) => _mediator = mediator;

    // Localized by ?lang / Accept-Language; NoStore so language variants aren't cross-served.
    [HttpGet]
    [ResponseCache(NoStore = true)]
    [ProducesResponseType(typeof(List<RoadSignCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRoadSignsQuery(), ct);
        return Ok(result);
    }
}
