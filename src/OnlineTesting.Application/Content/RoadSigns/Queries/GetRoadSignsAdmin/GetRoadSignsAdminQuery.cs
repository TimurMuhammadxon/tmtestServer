using MediatR;

namespace OnlineTesting.Application.Content.RoadSigns.Queries.GetRoadSignsAdmin;

public record GetRoadSignsAdminQuery : IRequest<List<AdminRoadSignCategoryDto>>;

public record AdminCategoryTranslationDto(string Language, string Name);
public record AdminSignTranslationDto(string Language, string Name, string? Description);

public record AdminRoadSignCategoryDto(
    Guid Id,
    int OrderIndex,
    List<AdminCategoryTranslationDto> Translations,
    List<AdminRoadSignDto> Signs);

public record AdminRoadSignDto(
    Guid Id,
    Guid CategoryId,
    string? Code,
    int OrderIndex,
    bool IsActive,
    string? ImageUrl,
    List<AdminSignTranslationDto> Translations);
