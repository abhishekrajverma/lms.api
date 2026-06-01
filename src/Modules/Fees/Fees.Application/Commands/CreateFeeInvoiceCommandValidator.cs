using FluentValidation;

namespace SchoolErp.Modules.Fees.Application.Commands;

public sealed class CreateFeeInvoiceCommandValidator : AbstractValidator<CreateFeeInvoiceCommand>
{
    public CreateFeeInvoiceCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.InvoiceNumber).NotEmpty();
    }
}
