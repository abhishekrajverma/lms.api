using LMS.SharedKernal.Results;
using LMS.Fee.API.Application.DTOs;
using MediatR;

namespace LMS.Fee.API.Application.Commands;

public sealed record CreateFeeInvoiceCommand(
    Guid StudentId,
    string StudentName,
    string ClassName,
    decimal TotalFee,
    DateOnly DueDate,
    decimal PaidAmount = 0) : IRequest<Result<FeeInvoiceDto>>;
