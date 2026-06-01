using FluentValidation;

namespace SchoolErp.Modules.Inventory.Application.Commands;

public sealed class CreateInventoryItemCommandValidator : AbstractValidator<CreateInventoryItemCommand>
{
    public CreateInventoryItemCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty();
    }
}
