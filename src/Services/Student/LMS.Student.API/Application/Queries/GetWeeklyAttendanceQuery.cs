using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using MediatR;

namespace LMS.Student.API.Application.Queries;

public sealed record GetWeeklyAttendanceQuery : IRequest<Result<IReadOnlyList<WeeklyAttendanceDto>>>;
