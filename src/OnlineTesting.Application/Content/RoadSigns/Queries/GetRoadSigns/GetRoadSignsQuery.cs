using MediatR;

namespace OnlineTesting.Application.Content.RoadSigns.Queries.GetRoadSigns;

public record GetRoadSignsQuery : IRequest<List<RoadSignCategoryDto>>;

public record RoadSignCategoryDto(
    Guid Id,
    int OrderIndex,
    string Name,
    List<RoadSignDto> Signs);

public record RoadSignDto(
    Guid Id,
    string? Code,
    string Name,
    string? Description,
    string? ImageUrl);
