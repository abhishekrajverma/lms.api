using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Attendance.Domain.Aggregates;
using SchoolErp.Modules.Attendance.Domain.Repositories;

namespace SchoolErp.Modules.Attendance.Application.Commands;

public sealed record CreateAttendanceSessionCommand(Guid TenantId, DateOnly SessionDate) : ICommand<Result<Guid>>;

public sealed class CreateAttendanceSessionCommandHandler : IRequestHandler<CreateAttendanceSessionCommand, Result<Guid>>
{
    private readonly IAttendanceSessionRepository _repository;

    public CreateAttendanceSessionCommandHandler(IAttendanceSessionRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateAttendanceSessionCommand request, CancellationToken cancellationToken)
    {
        var entity = AttendanceSession.Create(request.TenantId, request.SessionDate);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
