using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Students.Domain.Aggregates;

namespace SchoolErp.Modules.Students.Persistence;

public sealed class StudentsDbContext : BaseDbContext
{
    public StudentsDbContext(DbContextOptions<StudentsDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Student>(e =>
        {
            e.ToTable("Students", "Students");
            e.HasKey(x => x.Id);
        });
    }
}
