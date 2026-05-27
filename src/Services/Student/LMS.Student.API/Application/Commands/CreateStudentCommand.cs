using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using MediatR;

namespace LMS.Student.API.Application.Commands;

public sealed record CreateStudentCommand(
    string FirstName,
    string LastName,
    string AdmissionNumber,
    string? ClassName = null,
    string? Email = null,
    string? Phone = null,
    string? Status = null,
    string? FeeStatus = null,
    decimal? AttendancePercentage = null)
    : IRequest<Result<StudentDto>>;
