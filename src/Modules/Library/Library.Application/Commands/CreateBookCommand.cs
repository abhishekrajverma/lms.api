using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Library.Domain.Aggregates;
using SchoolErp.Modules.Library.Domain.Repositories;

namespace SchoolErp.Modules.Library.Application.Commands;

public sealed record CreateBookCommand(Guid TenantId, string Title, string? Isbn) : ICommand<Result<Guid>>;

public sealed class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Result<Guid>>
{
    private readonly IBookRepository _repository;

    public CreateBookCommandHandler(IBookRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var entity = Book.Create(request.TenantId, request.Title, request.Isbn);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
