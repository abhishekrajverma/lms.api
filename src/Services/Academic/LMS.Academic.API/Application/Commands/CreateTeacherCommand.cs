using LMS.Academic.API.Application.DTOs;
using LMS.Academic.API.Domain.Aggregates;
using LMS.Academic.API.Infrastructure.Persistence;
using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Academic.API.Application.Commands;

public sealed record CreateTeacherCommand(
    string FirstName,
    string LastName,
    string Department,
    string Subject,
    string Email,
    string Phone,
    decimal Salary,
    string? Status = null) : IRequest<Result<TeacherListItemDto>>;

public sealed class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, Result<TeacherListItemDto>>
{
    private readonly AcademicDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IUnitOfWork _uow;

    public CreateTeacherCommandHandler(AcademicDbContext db, ITenantContext tenant, IUnitOfWork uow)
    {
        _db = db;
        _tenant = tenant;
        _uow = uow;
    }

    public async Task<Result<TeacherListItemDto>> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        if (!_tenant.IsResolved)
            return Result.Failure<TeacherListItemDto>(Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var status = Enum.TryParse<TeacherStatus>(request.Status, true, out var parsed)
            ? parsed
            : TeacherStatus.Active;

        var teacher = Teacher.Hire(
            _tenant.TenantId,
            request.FirstName,
            request.LastName,
            request.Department,
            request.Subject,
            request.Email,
            request.Phone,
            request.Salary,
            status);

        await _db.Teachers.AddAsync(teacher, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Success(new TeacherListItemDto(
            teacher.Id,
            teacher.FullName,
            teacher.Department,
            teacher.Subject,
            teacher.Email,
            teacher.Phone,
            teacher.Salary,
            teacher.Status == TeacherStatus.OnLeave ? "on-leave" : teacher.Status.ToString().ToLowerInvariant()));
    }
}
