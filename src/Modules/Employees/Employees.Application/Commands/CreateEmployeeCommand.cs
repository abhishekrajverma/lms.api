using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Employees.Domain.Aggregates;
using SchoolErp.Modules.Employees.Domain.Repositories;

namespace SchoolErp.Modules.Employees.Application.Commands;

public sealed record CreateEmployeeCommand(Guid TenantId, string FirstName, string LastName) : ICommand<Result<Guid>>;

public sealed class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
{
    private readonly IEmployeeRepository _repository;

    public CreateEmployeeCommandHandler(IEmployeeRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var entity = Employee.Create(request.TenantId, request.FirstName, request.LastName);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
