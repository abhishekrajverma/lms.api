using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Examinations.Domain.Aggregates;
using SchoolErp.Modules.Examinations.Domain.Repositories;

namespace SchoolErp.Modules.Examinations.Application.Commands;

public sealed record CreateExamCommand(Guid TenantId, string Title) : ICommand<Result<Guid>>;

public sealed class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, Result<Guid>>
{
    private readonly IExamRepository _repository;

    public CreateExamCommandHandler(IExamRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateExamCommand request, CancellationToken cancellationToken)
    {
        var entity = Exam.Create(request.TenantId, request.Title);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
