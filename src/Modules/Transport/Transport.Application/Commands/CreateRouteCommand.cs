using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Transport.Domain.Aggregates;
using SchoolErp.Modules.Transport.Domain.Repositories;

namespace SchoolErp.Modules.Transport.Application.Commands;

public sealed record CreateRouteCommand(Guid TenantId, string Name) : ICommand<Result<Guid>>;

public sealed class CreateRouteCommandHandler : IRequestHandler<CreateRouteCommand, Result<Guid>>
{
    private readonly IRouteRepository _repository;

    public CreateRouteCommandHandler(IRouteRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
        var entity = Route.Create(request.TenantId, request.Name);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
