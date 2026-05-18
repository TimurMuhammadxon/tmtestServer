using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Teacher.Applications.Admin.Commands.ApproveApplication;
using OnlineTesting.Application.Teacher.Applications.Admin.Commands.RejectApplication;
using OnlineTesting.Application.Teacher.Applications.Admin.Queries.GetApplicationsList;
using OnlineTesting.Domain.Authorization;
using OnlineTesting.Domain.Teacher;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/teacher-applications")]
[Authorize(Policy = Roles.Policies.ContentManagement)]
public class AdminTeacherApplicationsController : ControllerBase
{
    private readonly ISender _sender;
    public AdminTeacherApplicationsController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<PagedResult<ApplicationListItemDto>> List(
        [FromQuery] TeacherApplicationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => _sender.Send(new GetApplicationsListQuery(status, page, pageSize), ct);

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ApproveTeacherApplicationCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectBody body, CancellationToken ct)
    {
        await _sender.Send(new RejectTeacherApplicationCommand(id, body.Reason), ct);
        return NoContent();
    }

    public record RejectBody(string? Reason);
}
