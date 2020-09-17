using Korga.Server.Configuration;
using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
            ConfigureEntityBase(person);

            var group = modelBuilder.Entity<Group>();
            group.HasKey(g => g.Id);
            ConfigureEntityBase(group);

            var groupRole = modelBuilder.Entity<GroupRole>();
            groupRole.HasKey(r => r.Id);
            groupRole.HasOne(r => r.Group).WithMany().HasForeignKey(r => r.GroupId);
            ConfigureEntityBase(groupRole);

            // Immutable entity → preserves history
            var groupMember = modelBuilder.Entity<GroupMember>();
            groupMember.HasKey(m => m.Id);
            groupMember.HasOne(m => m.Person).WithMany().HasForeignKey(m => m.PersonId);
            groupMember.HasOne(m => m.GroupRole).WithMany().HasForeignKey(m => m.GroupRoleId);
            ConfigureEntityBase(groupMember);

            var distributionList = modelBuilder.Entity<DistributionList>();
            distributionList.HasKey(l => l.Id);
            distributionList.HasAlternateKey(l => l.Alias);
            ConfigureEntityBase(distributionList);

            // Immutable entity → preserves history
            var receiveRole = modelBuilder.Entity<ReceiveRole>();
            receiveRole.HasKey(r => new { r.GroupRoleId, r.DistributionListId });
            receiveRole.HasOne(r => r.GroupRole).WithMany().HasForeignKey(r => r.GroupRoleId);
            receiveRole.HasOne(r => r.DistributionList).WithMany().HasForeignKey(r => r.DistributionListId);
            ConfigureEntityBase(receiveRole);

            var sendRole = modelBuilder.Entity<SendRole>();
            sendRole.HasKey(r => new { r.GroupRoleId, r.DistributionListId });
            sendRole.HasOne(r => r.GroupRole).WithMany().HasForeignKey(r => r.GroupRoleId);
            sendRole.HasOne(r => r.DistributionList).WithMany().HasForeignKey(r => r.DistributionListId);
            ConfigureEntityBase(sendRole);

            // Immutable entity → preserves history
            var message = modelBuilder.Entity<Message>();
            message.HasKey(m => m.Id);
            message.Property(m => m.ReceptionTime).HasDefaultValueSql(currentTimestamp);

            // Immutable entity → preserves history
            var messageAssignment = modelBuilder.Entity<MessageAssignment>();
            messageAssignment.HasKey(a => a.Id);
            messageAssignment.HasOne(a => a.Message).WithMany().HasForeignKey(a => a.MessageId);
            messageAssignment.HasOne(a => a.DistributionList).WithMany().HasForeignKey(a => a.DistributionListId);
            ConfigureEntityBase(messageAssignment);

            // Immutable entity → preserves history
            var messageReview = modelBuilder.Entity<MessageReview>();
            messageReview.HasKey(r => r.Id);
            messageReview.HasOne(r => r.MessageAssignment).WithMany().HasForeignKey(r => r.MessageAssignmentId);
            messageReview.HasOne(r => r.Person).WithMany().HasForeignKey(r => r.PersonId);
            messageReview.Property(r => r.CreationTime).HasDefaultValueSql(currentTimestamp);
        }

        /// <summary>
        /// Configures common properties for creation and deletion tracking.
        /// </summary>
        private static void ConfigureEntityBase(EntityTypeBuilder entityBuilder)
        {
            // Generics and LINQ expressions cannot be used for a base class
            entityBuilder.Property(nameof(EntityBase.CreationTime)).HasDefaultValueSql(currentTimestamp);
            entityBuilder.HasOne(nameof(EntityBase.Creator)).WithMany().HasForeignKey(nameof(EntityBase.CreatorId)).OnDelete(DeleteBehavior.SetNull);
            entityBuilder.HasOne(nameof(EntityBase.Deletor)).WithMany().HasForeignKey(nameof(EntityBase.DeletorId)).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
