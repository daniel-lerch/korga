using Microsoft.EntityFrameworkCore;

namespace Mailist.Tests.Migrations.ForwardMode;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<OutboxEmail> OutboxEmails => Set<OutboxEmail>();
}
