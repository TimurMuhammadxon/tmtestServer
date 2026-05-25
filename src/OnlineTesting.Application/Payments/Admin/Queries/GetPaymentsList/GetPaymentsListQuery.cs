using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Payments.Admin.Queries.GetPaymentsList;

public record GetPaymentsListQuery(int Page = 1, int PageSize = 30)
    : IRequest<PagedResult<PaymentOrderAdminDto>>;

public record PaymentOrderAdminDto(
    Guid Id,
    string UserEmail,
    string PlanLabel,
    long AmountTiyin,
    string Status,
    DateTime CreatedAt);
