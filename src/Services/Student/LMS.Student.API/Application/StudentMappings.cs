using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Domain.Aggregates;

namespace LMS.Student.API.Application;

internal static class StudentMappings
{
    public static StudentDto ToDto(StudentProfile student) =>
        new(
            student.Id,
            student.TenantId,
            student.FirstName,
            student.LastName,
            student.AdmissionNumber,
            student.ClassName,
            student.Email,
            student.Phone,
            student.Status.ToString().ToLowerInvariant(),
            student.FeeStatus.ToString().ToLowerInvariant(),
            student.AttendancePercentage,
            student.PresentDays,
            student.AbsentDays,
            student.LateDays,
            student.EnrolledAtUtc);

    public static StudentListItemDto ToListItem(StudentProfile student) =>
        new(
            student.Id,
            student.FullName,
            student.ClassName,
            student.AdmissionNumber,
            student.Email,
            student.Phone,
            student.Status.ToString().ToLowerInvariant(),
            student.FeeStatus.ToString().ToLowerInvariant(),
            student.AttendancePercentage);
}
