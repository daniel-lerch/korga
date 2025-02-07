using Microsoft.EntityFrameworkCore;

namespace Korga.Tests.Migrations.NullableEmailHeaders;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<DistributionList> DistributionLists => Set<DistributionList>();
    public DbSet<PersonFilterList> PersonFilterLists => Set<PersonFilterList>();
    public DbSet<InboxEmail> InboxEmails => Set<InboxEmail>();
}
