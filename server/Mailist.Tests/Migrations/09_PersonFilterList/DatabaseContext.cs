using Microsoft.EntityFrameworkCore;

namespace Mailist.Tests.Migrations.PersonFilterList;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<InboxEmail> InboxEmails => Set<InboxEmail>();
    public DbSet<DistributionList> DistributionLists => Set<DistributionList>();
    public DbSet<PersonFilterList> PersonFilterLists => Set<PersonFilterList>();
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
        modelBuilder.Entity<GroupFilter>()
            .Property(f => f.GroupRoleId).HasColumnName(nameof(GroupFilter.GroupRoleId));
        modelBuilder.Entity<GroupTypeFilter>()
            .Property(f => f.GroupRoleId).HasColumnName(nameof(GroupTypeFilter.GroupRoleId));
        modelBuilder.Entity<StatusFilter>();
        modelBuilder.Entity<SinglePerson>();
    }
}
