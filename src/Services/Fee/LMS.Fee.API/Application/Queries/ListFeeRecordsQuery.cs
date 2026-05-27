using LMS.Fee.API.Application.DTOs;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Fee.API.Application.Queries;

public sealed record ListFeeRecordsQuery(string? Status = null, string? Search = null)
    : IRequest<Result<IReadOnlyList<FeeInvoiceDto>>>;
