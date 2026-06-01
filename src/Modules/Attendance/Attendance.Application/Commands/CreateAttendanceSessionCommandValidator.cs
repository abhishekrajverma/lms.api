using FluentValidation;

namespace SchoolErp.Modules.Attendance.Application.Commands;

public sealed class CreateAttendanceSessionCommandValidator : AbstractValidator<CreateAttendanceSessionCommand>
{
    public CreateAttendanceSessionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SessionDate).NotEmpty();
    }
}
