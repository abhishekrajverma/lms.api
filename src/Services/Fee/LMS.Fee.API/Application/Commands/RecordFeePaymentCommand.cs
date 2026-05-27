using LMS.Fee.API.Application.DTOs;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Fee.API.Application.Commands;

public sealed record RecordFeePaymentCommand(Guid InvoiceId) : IRequest<Result<FeeInvoiceDto>>;
