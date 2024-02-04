using Microsoft.EntityFrameworkCore;

namespace Korga.Server.Tests.Migrations.AddSinglePersonFilter;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Email> Emails => Set<Email>();
}
