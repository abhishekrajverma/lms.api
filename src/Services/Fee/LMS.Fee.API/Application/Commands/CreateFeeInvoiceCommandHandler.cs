using LMS.Fee.API.Application.DTOs;
using LMS.Fee.API.Domain.Aggregates;
using LMS.Fee.API.Domain.Repositories;
using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Fee.API.Application.Commands;

public sealed class CreateFeeInvoiceCommandHandler : IRequestHandler<CreateFeeInvoiceCommand, Result<FeeInvoiceDto>>
{
    private readonly IFeeInvoiceRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFeeInvoiceCommandHandler(
        IFeeInvoiceRepository repository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FeeInvoiceDto>> Handle(CreateFeeInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<FeeInvoiceDto>(Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var invoice = FeeInvoice.Create(
            _tenantContext.TenantId,
            request.StudentId,
            request.StudentName,
            request.ClassName,
            request.TotalFee,
            request.DueDate,
            request.PaidAmount);

        await _repository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(FeeMappings.ToDto(invoice));
    }
}
