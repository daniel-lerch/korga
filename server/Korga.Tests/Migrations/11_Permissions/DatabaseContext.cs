using Microsoft.EntityFrameworkCore;

namespace Korga.Tests.Migrations.Permissions;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<DistributionList> DistributionLists => Set<DistributionList>();
    public DbSet<PersonFilterList> PersonFilterLists => Set<PersonFilterList>();
    public DbSet<Permission> Permissions => Set<Permission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var permission = modelBuilder.Entity<Permission>();
        permission.HasKey(p => p.Key);
        permission.Property(p => p.Key).HasConversion<string>().ValueGeneratedNever();
    }
}
