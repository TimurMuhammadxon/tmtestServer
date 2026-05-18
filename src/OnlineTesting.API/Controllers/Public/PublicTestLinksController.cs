using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.TestLinks.Commands.StartTestLink;
using OnlineTesting.Application.TestLinks.Queries.GetTestLinkInfo;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("test-links")]
[Authorize]
public class PublicTestLinksController : ControllerBase
{
    private readonly ISender _sender;
    public PublicTestLinksController(ISender sender) => _sender = sender;

    [HttpGet("{code}")]
    public Task<TestLinkInfoDto> GetInfo(string code, CancellationToken ct)
        => _sender.Send(new GetTestLinkInfoQuery(code), ct);

    [HttpPost("{code}/start")]
    public async Task<IActionResult> Start(string code, CancellationToken ct)
    {
        var attemptId = await _sender.Send(new StartTestLinkCommand(code), ct);
        return Ok(new { attemptId });
    }
}
