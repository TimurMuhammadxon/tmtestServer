using Microsoft.AspNetCore.Authorization;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Authorization;

public class TeacherSubscriptionRequirement : IAuthorizationRequirement { }

public class TeacherSubscriptionHandler : AuthorizationHandler<TeacherSubscriptionRequirement>
{
    private readonly ISubscriptionChecker _subscription;
    private readonly ICurrentUser _currentUser;

    public TeacherSubscriptionHandler(ISubscriptionChecker subscription, ICurrentUser currentUser)
    {
        _subscription = subscription;
        _currentUser = currentUser;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TeacherSubscriptionRequirement requirement)
    {
        // Owner, SuperAdmin, Admin always pass — no subscription required
        if (context.User.IsInRole(Roles.Owner) ||
            context.User.IsInRole(Roles.SuperAdmin) ||
            context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = _currentUser.UserId;
        if (userId is null)
        {
            context.Fail();
            return;
        }

        if (await _subscription.IsTeacherSubscriptionActiveAsync(userId.Value, CancellationToken.None))
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
