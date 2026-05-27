using LMS.SharedKernal.Primitives;
using LMS.Student.API.Domain.Aggregates;

namespace LMS.Student.API.Domain.Repositories;

public interface IStudentRepository : IRepository<StudentProfile>
{
}
