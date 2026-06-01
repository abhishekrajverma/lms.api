using FluentValidation;

namespace SchoolErp.Modules.Students.Application.Commands;

public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty();
    }
}
