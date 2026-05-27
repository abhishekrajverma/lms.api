using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Domain.Aggregates;
using LMS.Student.API.Domain.Repositories;
using MediatR;

namespace LMS.Student.API.Application.Commands;

public sealed class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<StudentDto>>
{
    private readonly IStudentRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStudentCommandHandler(
        IStudentRepository repository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StudentDto>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<StudentDto>(Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var student = StudentProfile.Enroll(
            _tenantContext.TenantId,
            request.FirstName,
            request.LastName,
            request.AdmissionNumber,
            request.ClassName,
            request.Email,
            request.Phone,
            ParseStatus(request.Status),
            ParseFeeStatus(request.FeeStatus),
            request.AttendancePercentage ?? 0);

        await _repository.AddAsync(student, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(StudentMappings.ToDto(student));
    }

    private static StudentStatus ParseStatus(string? status) =>
        Enum.TryParse<StudentStatus>(status, true, out var parsed) ? parsed : StudentStatus.Active;

    private static StudentFeeStatus ParseFeeStatus(string? feeStatus) =>
        Enum.TryParse<StudentFeeStatus>(feeStatus, true, out var parsed) ? parsed : StudentFeeStatus.Pending;
}
