using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using MediatR;

namespace LMS.Student.API.Application.Queries;

public sealed record GetRecentAdmissionsQuery(int Limit = 10) : IRequest<Result<IReadOnlyList<RecentAdmissionDto>>>;
