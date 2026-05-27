using LMS.Fee.API.Application.DTOs;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Fee.API.Application.Queries;

public sealed record GetFeeSummaryQuery : IRequest<Result<FeeSummaryDto>>;
