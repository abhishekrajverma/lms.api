using LMS.Fee.API.Application.DTOs;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Fee.API.Application.Queries;

public sealed record GetRecentFeePaymentsQuery(int Limit = 5) : IRequest<Result<IReadOnlyList<RecentFeePaymentDto>>>;
