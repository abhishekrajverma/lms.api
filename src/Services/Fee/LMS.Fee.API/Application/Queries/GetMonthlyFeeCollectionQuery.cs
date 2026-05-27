using LMS.Fee.API.Application.DTOs;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Fee.API.Application.Queries;

public sealed record GetMonthlyFeeCollectionQuery(int Months = 6) : IRequest<Result<IReadOnlyList<MonthlyFeeCollectionDto>>>;
