using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Library.Domain.Aggregates;

namespace SchoolErp.Modules.Library.Persistence;

public sealed class LibraryDbContext : BaseDbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Book>(e =>
        {
            e.ToTable("Books", "Library");
            e.HasKey(x => x.Id);
        });
    }
}
