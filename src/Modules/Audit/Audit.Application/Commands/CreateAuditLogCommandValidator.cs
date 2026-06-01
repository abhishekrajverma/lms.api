using FluentValidation;

namespace SchoolErp.Modules.Audit.Application.Commands;

public sealed class CreateAuditLogCommandValidator : AbstractValidator<CreateAuditLogCommand>
{
    public CreateAuditLogCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty();
    }
}
