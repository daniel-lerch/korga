using Korga.Server.Configuration;
using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Korga.Server.Database
{
    public class DatabaseContext : DbContext
    {
        private readonly IOptions<DatabaseOptions> options;

        // These properties are automatically assigned by EF Core
        public DbSet<Person> People { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;

        public DatabaseContext(IOptions<DatabaseOptions> options)
        {
            this.options = options;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseMySql(options.Value.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var person = modelBuilder.Entity<Person>();
            person.HasKey(p => p.Id);

            var account = modelBuilder.Entity<Account>();
            account.HasKey(a => a.Id);
            account.HasOne(a => a.Person).WithOne(p => p!.Account!).HasForeignKey<Account>(a => a.PersonId);
        }
    }
}
