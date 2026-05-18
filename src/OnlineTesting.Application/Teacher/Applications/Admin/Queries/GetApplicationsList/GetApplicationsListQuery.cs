using MediatR;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Teacher;

namespace OnlineTesting.Application.Teacher.Applications.Admin.Queries.GetApplicationsList;

public record GetApplicationsListQuery(
    TeacherApplicationStatus? Status,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ApplicationListItemDto>>;

public record ApplicationListItemDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string FullName,
    string PhoneNumber,
    string? OrganizationName,
    string Status,
    DateTime SubmittedAt,
    DateTime? ReviewedAt);
