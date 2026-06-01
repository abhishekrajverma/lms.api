using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.Identity.Domain.Aggregates;

namespace SchoolErp.Modules.Identity.Persistence;

public sealed class IdentityDbContext : BaseDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users", "Identity");
            e.HasKey(x => x.Id);
        });
    }
}
