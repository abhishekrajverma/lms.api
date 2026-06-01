using FluentValidation;

namespace SchoolErp.Modules.Communication.Application.Commands;

public sealed class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty();
    }
}
