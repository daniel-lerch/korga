using Microsoft.EntityFrameworkCore;

namespace Korga.Tests.Migrations.InboxOutbox;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<InboxEmail> InboxEmails => Set<InboxEmail>();
}
