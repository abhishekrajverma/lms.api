using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Domain.Repositories;
using MediatR;

namespace LMS.Student.API.Application.Queries;

public sealed class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
{
    private readonly IStudentRepository _repository;

    public GetStudentByIdQueryHandler(IStudentRepository repository) => _repository = repository;

    public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (student is null)
            return Result.Failure<StudentDto>(Error.NotFound("Student.NotFound", "Student was not found."));

        return Result.Success(StudentMappings.ToDto(student));
    }
}
