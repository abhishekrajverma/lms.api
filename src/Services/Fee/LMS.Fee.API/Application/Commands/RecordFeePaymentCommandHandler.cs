using LMS.Fee.API.Application;
using LMS.Fee.API.Application.DTOs;
using LMS.Fee.API.Domain.Repositories;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Fee.API.Application.Commands;

public sealed class RecordFeePaymentCommandHandler : IRequestHandler<RecordFeePaymentCommand, Result<FeeInvoiceDto>>
{
    private readonly IFeeInvoiceRepository _repository;
    private readonly LMS.SharedKernal.Primitives.IUnitOfWork _uow;

    public RecordFeePaymentCommandHandler(IFeeInvoiceRepository repository, LMS.SharedKernal.Primitives.IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task<Result<FeeInvoiceDto>> Handle(RecordFeePaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result.Failure<FeeInvoiceDto>(Error.NotFound("Fee.NotFound", "Invoice not found."));

        invoice.MarkPaid();
        _repository.Update(invoice);
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success(FeeMappings.ToDto(invoice));
    }
}
