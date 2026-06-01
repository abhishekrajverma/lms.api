using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Academics.Domain.Aggregates;

namespace SchoolErp.Modules.Academics.Persistence;

public sealed class AcademicsDbContext : BaseDbContext
{
    public AcademicsDbContext(DbContextOptions<AcademicsDbContext> options) : base(options) { }

    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AcademicYear>(e =>
        {
            e.ToTable("AcademicYears", "Academics");
            e.HasKey(x => x.Id);
        });
    }
}
