using Microsoft.EntityFrameworkCore;

namespace Korga.Tests.Migrations.ForwardMode;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<OutboxEmail> OutboxEmails => Set<OutboxEmail>();
}
