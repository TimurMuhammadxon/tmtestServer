using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Teacher.Groups.Commands.CreateGroup;
using OnlineTesting.Application.Teacher.Groups.Commands.DeleteGroup;
using OnlineTesting.Application.Teacher.Groups.Commands.RemoveMember;
using OnlineTesting.Application.Teacher.Groups.Queries.GetGroupMembers;
using OnlineTesting.Application.Teacher.Groups.Queries.GetGroups;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Teacher;

[ApiController]
[Route("teacher/groups")]
[Authorize(Policy = Roles.Policies.TeacherSubscriptionAccess)]
public class TeacherGroupsController : ControllerBase
{
    private readonly ISender _sender;
    public TeacherGroupsController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<List<GroupDto>> List(CancellationToken ct)
        => _sender.Send(new GetGroupsQuery(), ct);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetMembers), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteGroupCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    public Task<List<GroupMemberDto>> GetMembers(Guid id, CancellationToken ct)
        => _sender.Send(new GetGroupMembersQuery(id), ct);

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken ct)
    {
        await _sender.Send(new RemoveMemberCommand(id, userId), ct);
        return NoContent();
    }
}
