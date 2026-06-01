using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Students.Domain.Aggregates;
using SchoolErp.Modules.Students.Domain.Repositories;

namespace SchoolErp.Modules.Students.Application.Commands;

public sealed record CreateStudentCommand(Guid TenantId, string FirstName, string LastName) : ICommand<Result<Guid>>;

public sealed class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _repository;

    public CreateStudentCommandHandler(IStudentRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var entity = Student.Create(request.TenantId, request.FirstName, request.LastName);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
