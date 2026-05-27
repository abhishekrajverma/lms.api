using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using MediatR;

namespace LMS.Student.API.Application.Queries;

public sealed record GetStudentAttendanceRowsQuery(
    string? Class = null,
    string? Search = null) : IRequest<Result<IReadOnlyList<StudentAttendanceRowDto>>>;
