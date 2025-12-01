using Microsoft.EntityFrameworkCore;

namespace Mailist.Tests.Migrations.NullableEmailHeaders;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<InboxEmail> InboxEmails => Set<InboxEmail>();
}
