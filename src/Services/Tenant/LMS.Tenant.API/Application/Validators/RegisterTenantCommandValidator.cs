using FluentValidation;
using LMS.Tenant.API.Application.Commands;

namespace LMS.Tenant.API.Application.Validators;

public sealed class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subdomain).NotEmpty().MaximumLength(100).Matches("^[a-z0-9-]+$");
    }
}
