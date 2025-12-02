using Microsoft.EntityFrameworkCore;

namespace Mailist.Tests.Migrations.SplitOutboxEmail;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupType> GroupTypes => Set<GroupType>();
    public DbSet<OutboxEmail> OutboxEmails => Set<OutboxEmail>();
    public DbSet<SentEmail> SentEmails => Set<SentEmail>();
}
