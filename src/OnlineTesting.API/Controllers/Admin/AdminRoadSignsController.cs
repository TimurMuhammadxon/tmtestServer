using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Content.RoadSigns.Commands;
using OnlineTesting.Application.Content.RoadSigns.Queries.GetRoadSignsAdmin;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/road-signs")]
[Authorize(Policy = Roles.Policies.ContentManagement)]
public class AdminRoadSignsController : ControllerBase
{
    private readonly ISender _sender;
    public AdminRoadSignsController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<List<AdminRoadSignCategoryDto>> List(CancellationToken ct)
        => _sender.Send(new GetRoadSignsAdminQuery(), ct);

    // ── Categories ──────────────────────────────────────────────
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateRoadSignCategoryCommand body, CancellationToken ct)
    {
        var id = await _sender.Send(body, ct);
        return Ok(new { id });
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryBody body, CancellationToken ct)
    {
        await _sender.Send(new UpdateRoadSignCategoryCommand(id, body.OrderIndex, body.Translations), ct);
        return NoContent();
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteRoadSignCategoryCommand(id), ct);
        return NoContent();
    }

    // ── Signs ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> CreateSign([FromBody] CreateRoadSignCommand body, CancellationToken ct)
    {
        var id = await _sender.Send(body, ct);
        return Ok(new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSign(Guid id, [FromBody] UpdateSignBody body, CancellationToken ct)
    {
        await _sender.Send(new UpdateRoadSignCommand(id, body.CategoryId, body.Code, body.OrderIndex, body.IsActive, body.Translations), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSign(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteRoadSignCommand(id), ct);
        return NoContent();
    }

    // ── Sign image ──────────────────────────────────────────────
    [HttpPost("{id:guid}/image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadRoadSignImageResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");
        const long maxSize = 5 * 1024 * 1024;
        if (file.Length > maxSize)
            return BadRequest("File size must not exceed 5 MB.");

        using var stream = file.OpenReadStream();
        var result = await _sender.Send(new UploadRoadSignImageCommand(id, stream, file.ContentType), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/image")]
    public async Task<IActionResult> DeleteImage(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteRoadSignImageCommand(id), ct);
        return NoContent();
    }

    public record UpdateCategoryBody(int OrderIndex, List<SignTranslationInput> Translations);
    public record UpdateSignBody(Guid CategoryId, string? Code, int OrderIndex, bool IsActive, List<SignFullTranslationInput> Translations);
}
