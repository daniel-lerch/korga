using Microsoft.EntityFrameworkCore;

namespace Korga.Server.Tests.Migrations.GroupStatus;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupType> GroupTypes => Set<GroupType>();
    public DbSet<GroupStatus> GroupStatuses => Set<GroupStatus>();
}
