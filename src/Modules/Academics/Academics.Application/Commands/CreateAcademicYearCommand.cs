using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Academics.Domain.Aggregates;
using SchoolErp.Modules.Academics.Domain.Repositories;

namespace SchoolErp.Modules.Academics.Application.Commands;

public sealed record CreateAcademicYearCommand(Guid TenantId, string Name, DateOnly StartDate, DateOnly EndDate) : ICommand<Result<Guid>>;

public sealed class CreateAcademicYearCommandHandler : IRequestHandler<CreateAcademicYearCommand, Result<Guid>>
{
    private readonly IAcademicYearRepository _repository;

    public CreateAcademicYearCommandHandler(IAcademicYearRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateAcademicYearCommand request, CancellationToken cancellationToken)
    {
        var entity = AcademicYear.Create(request.TenantId, request.Name, request.StartDate, request.EndDate);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
