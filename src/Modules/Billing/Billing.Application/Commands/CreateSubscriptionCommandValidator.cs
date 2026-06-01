using FluentValidation;

namespace SchoolErp.Modules.Billing.Application.Commands;

public sealed class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PlanCode).NotEmpty();
    }
}
