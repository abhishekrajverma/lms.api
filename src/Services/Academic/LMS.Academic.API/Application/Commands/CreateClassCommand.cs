using LMS.Academic.API.Application.DTOs;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Academic.API.Application.Commands;

public sealed record CreateClassCommand(string Name, string Grade) : IRequest<Result<ClassDto>>;
