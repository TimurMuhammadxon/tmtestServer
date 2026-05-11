using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Tests.Solutions.Queries.GetQuestionsByTopic;
using OnlineTesting.Application.Tests.Solutions.Queries.GetTopicsForStudent;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("topics")]
public class PublicTopicsController : ControllerBase
{
    private readonly ISender _sender;

    public PublicTopicsController(ISender sender) => _sender = sender;

    [AllowAnonymous]
    [HttpGet]
    public Task<List<TopicStudentDto>> List(CancellationToken ct)
    {
        var guest = User.Identity?.IsAuthenticated != true;
        return _sender.Send(new GetTopicsForStudentQuery(GuestMode: guest), ct);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/questions")]
    public Task<PagedResult<QuestionStudentDto>> GetQuestions(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var guest = User.Identity?.IsAuthenticated != true;
        return _sender.Send(new GetQuestionsByTopicQuery(id, guest, page, pageSize), ct);
    }
}