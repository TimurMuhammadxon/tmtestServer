using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Subscriptions.Queries.GetMySubscription;
using OnlineTesting.Application.Subscriptions.Queries.GetPlans;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("subscriptions")]
public class SubscriptionController : ControllerBase
{
    private readonly ISender _sender;
    public SubscriptionController(ISender sender) => _sender = sender;

    [HttpGet("plans")]
    [AllowAnonymous]
    [ResponseCache(Duration = 600)]
    public Task<List<SubscriptionPlanDto>> GetPlans(CancellationToken ct)
        => _sender.Send(new GetPlansQuery(), ct);

    [HttpGet("my")]
    [Authorize]
    public Task<MySubscriptionDto?> GetMy(CancellationToken ct)
        => _sender.Send(new GetMySubscriptionQuery(), ct);
}
