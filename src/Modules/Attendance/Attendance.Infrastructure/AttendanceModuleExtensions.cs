using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Attendance.Domain.Repositories;
using SchoolErp.Modules.Attendance.Persistence;
using SchoolErp.Modules.Attendance.Persistence.Repositories;

namespace SchoolErp.Modules.Attendance.Infrastructure;

public static class AttendanceModuleExtensions
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AttendanceDbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "Attendance"));

        services.AddScoped<IAttendanceSessionRepository, AttendanceSessionRepository>();
        return services;
    }
}
