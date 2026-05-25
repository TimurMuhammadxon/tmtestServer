using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Payments.Admin.Queries.GetPaymentsList;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/payments")]
[Authorize(Policy = Roles.Policies.OwnerAccess)]
public class AdminPaymentsController : ControllerBase
{
    private readonly ISender _sender;
    public AdminPaymentsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetPaymentsListQuery(page, pageSize), ct);
        return Ok(result);
    }
}
