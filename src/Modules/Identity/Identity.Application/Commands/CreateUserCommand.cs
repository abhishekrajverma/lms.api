using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Identity.Domain.Aggregates;
using SchoolErp.Modules.Identity.Domain.Repositories;

namespace SchoolErp.Modules.Identity.Application.Commands;

public sealed record CreateUserCommand(Guid TenantId, string Email) : ICommand<Result<Guid>>;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _repository;

    public CreateUserCommandHandler(IUserRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = User.Create(request.TenantId, request.Email);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
