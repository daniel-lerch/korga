using Microsoft.EntityFrameworkCore;

namespace Mailist.Tests.Migrations.AddSinglePersonFilter;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Email> Emails => Set<Email>();
}
