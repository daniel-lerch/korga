using Korga.Server.Configuration;
using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Korga.Server.Database
{
    public class DatabaseContext : DbContext
    {
        private const string currentTimestamp = "CURRENT_TIMESTAMP(6)";
        private readonly IOptions<DatabaseOptions> options;

        public DbSet<Person> People => Set<Person>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<GroupRole> GroupRoles => Set<GroupRole>();
        public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
        public DbSet<DistributionList> DistributionLists => Set<DistributionList>();
        public DbSet<ReceiveRole> ReceiveRoles => Set<ReceiveRole>();
        public DbSet<SendRole> SendRoles => Set<SendRole>();

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

            // TODO: Implement a versioning strategy to show changes for all entities

            var person = modelBuilder.Entity<Person>();
            person.HasKey(p => p.Id);
            person.Property(p => p.CreationTime).HasDefaultValueSql(currentTimestamp);

            var group = modelBuilder.Entity<Group>();
            group.HasKey(g => g.Id);
            group.Property(g => g.Name).IsRequired();
            group.Property(g => g.CreationTime).HasDefaultValueSql(currentTimestamp);

            var groupRole = modelBuilder.Entity<GroupRole>();
            groupRole.HasKey(r => r.Id);
            groupRole.HasOne(r => r.Group).WithMany().HasForeignKey(r => r.GroupId);
            groupRole.Property(r => r.Name).IsRequired();
            groupRole.Property(r => r.CreationTime).HasDefaultValueSql(currentTimestamp);

            var groupMember = modelBuilder.Entity<GroupMember>();
            groupMember.HasKey(m => new { m.PersonId, m.GroupRoleId });
            groupMember.HasOne(m => m.Person).WithMany().HasForeignKey(m => m.PersonId);
            groupMember.HasOne(m => m.GroupRole).WithMany().HasForeignKey(m => m.GroupRoleId);
            groupMember.Property(m => m.AccessionTime).HasDefaultValueSql(currentTimestamp);

            var distributionList = modelBuilder.Entity<DistributionList>();
            distributionList.HasKey(l => l.Id);
            distributionList.HasAlternateKey(l => l.Alias);
            distributionList.Property(l => l.Alias).IsRequired();
            distributionList.Property(l => l.Name).IsRequired();

            var receiveRole = modelBuilder.Entity<ReceiveRole>();
            receiveRole.HasKey(r => new { r.GroupRoleId, r.DistributionListId });
            receiveRole.HasOne(r => r.GroupRole).WithMany().HasForeignKey(r => r.GroupRoleId);
            receiveRole.HasOne(r => r.DistributionList).WithMany().HasForeignKey(r => r.DistributionListId);

            var sendRole = modelBuilder.Entity<SendRole>();
            sendRole.HasKey(r => new { r.GroupRoleId, r.DistributionListId });
            sendRole.HasOne(r => r.GroupRole).WithMany().HasForeignKey(r => r.GroupRoleId);
            sendRole.HasOne(r => r.DistributionList).WithMany().HasForeignKey(r => r.DistributionListId);

            // TODO: Add entities for messages and moderation
        }
    }
}
