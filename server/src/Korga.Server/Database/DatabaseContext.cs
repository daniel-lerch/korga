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
            group.Property(g => g.CreationTime).HasDefaultValueSql(currentTimestamp);

            var groupRole = modelBuilder.Entity<GroupRole>();
            groupRole.HasKey(r => r.Id);
            groupRole.HasOne(r => r.Group).WithMany().HasForeignKey(r => r.GroupId);
            groupRole.Property(r => r.CreationTime).HasDefaultValueSql(currentTimestamp);

            // Immutable entity → preserves history
            var groupMember = modelBuilder.Entity<GroupMember>();
            groupMember.HasKey(m => m.Id);
            groupMember.HasOne(m => m.Person).WithMany().HasForeignKey(m => m.PersonId);
            groupMember.HasOne(m => m.GroupRole).WithMany().HasForeignKey(m => m.GroupRoleId);
            groupMember.Property(m => m.AccessionTime).HasDefaultValueSql(currentTimestamp);
            groupMember.HasOne(m => m.Accessor).WithMany().HasForeignKey(m => m.AccessorId).OnDelete(DeleteBehavior.SetNull);
            groupMember.HasOne(m => m.Resignator).WithMany().HasForeignKey(m => m.ResignatorId).OnDelete(DeleteBehavior.SetNull);

            var distributionList = modelBuilder.Entity<DistributionList>();
            distributionList.HasKey(l => l.Id);
            distributionList.HasAlternateKey(l => l.Alias);

            var receiveRole = modelBuilder.Entity<ReceiveRole>();
            receiveRole.HasKey(r => new { r.GroupRoleId, r.DistributionListId });
            receiveRole.HasOne(r => r.GroupRole).WithMany().HasForeignKey(r => r.GroupRoleId);
            receiveRole.HasOne(r => r.DistributionList).WithMany().HasForeignKey(r => r.DistributionListId);

            var sendRole = modelBuilder.Entity<SendRole>();
            sendRole.HasKey(r => new { r.GroupRoleId, r.DistributionListId });
            sendRole.HasOne(r => r.GroupRole).WithMany().HasForeignKey(r => r.GroupRoleId);
            sendRole.HasOne(r => r.DistributionList).WithMany().HasForeignKey(r => r.DistributionListId);

            // Immutable entity → preserves history
            var message = modelBuilder.Entity<Message>();
            message.HasKey(m => m.Id);
            message.Property(m => m.ReceptionTime).HasDefaultValueSql(currentTimestamp);

            // Immutable entity → preserves history
            var messageAssignment = modelBuilder.Entity<MessageAssignment>();
            messageAssignment.HasKey(a => a.Id);
            messageAssignment.HasOne(a => a.Message).WithMany().HasForeignKey(a => a.MessageId);
            messageAssignment.HasOne(a => a.DistributionList).WithMany().HasForeignKey(a => a.DistributionListId);
            messageAssignment.Property(a => a.CreationTime).HasDefaultValueSql(currentTimestamp);
            messageAssignment.HasOne(a => a.Creator).WithMany().HasForeignKey(a => a.CreatorId).OnDelete(DeleteBehavior.SetNull);
            messageAssignment.HasOne(a => a.Deletor).WithMany().HasForeignKey(a => a.DeletorId).OnDelete(DeleteBehavior.SetNull);

            // Immutable entity → preserves history
            var messageReview = modelBuilder.Entity<MessageReview>();
            messageReview.HasKey(r => r.Id);
            messageReview.HasOne(r => r.MessageAssignment).WithMany().HasForeignKey(r => r.MessageAssignmentId);
            messageReview.HasOne(r => r.Person).WithMany().HasForeignKey(r => r.PersonId);
            messageReview.Property(r => r.CreationTime).HasDefaultValueSql(currentTimestamp);
        }
    }
}
