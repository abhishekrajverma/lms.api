using FluentValidation;
using LMS.Student.API.Application.Commands;

namespace LMS.Student.API.Application.Validators;

public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdmissionNumber).NotEmpty().MaximumLength(50);
    }
}
