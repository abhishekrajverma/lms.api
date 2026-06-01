using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Employees.Domain.Aggregates;

namespace SchoolErp.Modules.Employees.Persistence;

public sealed class EmployeesDbContext : BaseDbContext
{
    public EmployeesDbContext(DbContextOptions<EmployeesDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("Employees", "Employees");
            e.HasKey(x => x.Id);
        });
    }
}
