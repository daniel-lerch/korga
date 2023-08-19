using Microsoft.EntityFrameworkCore;

namespace Korga.Server.Tests.Migrations.SplitOutboxEmail;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<OutboxEmail> OutboxEmails => Set<OutboxEmail>();
    public DbSet<SentEmail> SentEmails => Set<SentEmail>();
}
