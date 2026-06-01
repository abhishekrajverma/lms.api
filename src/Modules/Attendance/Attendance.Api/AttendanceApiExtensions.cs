using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.Attendance.Infrastructure;

namespace SchoolErp.Modules.Attendance.Api;

public static class AttendanceApiExtensions
{
    public static IServiceCollection AddAttendanceApi(this IServiceCollection services, IConfiguration configuration) =>
        services.AddAttendanceModule(configuration);
}
