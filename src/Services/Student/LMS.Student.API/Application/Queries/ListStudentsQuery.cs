using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using MediatR;

namespace LMS.Student.API.Application.Queries;

public sealed record ListStudentsQuery(
    string? Search = null,
    string? Class = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<PagedStudentsDto>>;
