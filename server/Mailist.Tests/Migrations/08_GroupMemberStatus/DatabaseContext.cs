using Microsoft.EntityFrameworkCore;

namespace Mailist.Tests.Migrations.GroupMemberStatus;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<DistributionList> DistributionLists => Set<DistributionList>();
    public DbSet<PersonFilter> PersonFilters => Set<PersonFilter>();

    public DbSet<Person> People => Set<Person>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupRole> GroupRoles => Set<GroupRole>();
    public DbSet<GroupType> GroupTypes => Set<GroupType>();
    public DbSet<GroupStatus> GroupStatuses => Set<GroupStatus>();
    public DbSet<Status> Status => Set<Status>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PersonFilter>();
        modelBuilder.Entity<GroupFilter>();
        modelBuilder.Entity<StatusFilter>();
        modelBuilder.Entity<SinglePerson>();
    }
}
