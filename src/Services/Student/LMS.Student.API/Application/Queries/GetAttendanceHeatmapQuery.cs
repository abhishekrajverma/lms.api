using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using MediatR;

namespace LMS.Student.API.Application.Queries;

public sealed record GetAttendanceHeatmapQuery(int Months = 6) : IRequest<Result<IReadOnlyList<AttendanceHeatmapCellDto>>>;
