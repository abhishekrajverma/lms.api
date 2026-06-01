using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Examinations.Domain.Aggregates;

namespace SchoolErp.Modules.Examinations.Persistence;

public sealed class ExaminationsDbContext : BaseDbContext
{
    public ExaminationsDbContext(DbContextOptions<ExaminationsDbContext> options) : base(options) { }

    public DbSet<Exam> Exams => Set<Exam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Exam>(e =>
        {
            e.ToTable("Exams", "Examinations");
            e.HasKey(x => x.Id);
        });
    }
}
