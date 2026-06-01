using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Fees.Domain.Aggregates;
using SchoolErp.Modules.Fees.Domain.Repositories;

namespace SchoolErp.Modules.Fees.Application.Commands;

public sealed record CreateFeeInvoiceCommand(Guid TenantId, string InvoiceNumber) : ICommand<Result<Guid>>;

public sealed class CreateFeeInvoiceCommandHandler : IRequestHandler<CreateFeeInvoiceCommand, Result<Guid>>
{
    private readonly IFeeInvoiceRepository _repository;

    public CreateFeeInvoiceCommandHandler(IFeeInvoiceRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateFeeInvoiceCommand request, CancellationToken cancellationToken)
    {
        var entity = FeeInvoice.Create(request.TenantId, request.InvoiceNumber);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
