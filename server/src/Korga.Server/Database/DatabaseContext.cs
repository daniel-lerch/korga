using Korga.Server.Configuration;
using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Korga.Server.Database
{
    public sealed partial class DatabaseContext : DbContext
    {
        private const string currentTimestamp = "CURRENT_TIMESTAMP(6)";
        private readonly IOptions<DatabaseOptions> options;
        private readonly ILoggerFactory loggerFactory;

        public DbSet<Person> People => Set<Person>();
        public DbSet<PersonSnapshot> PersonSnapshots => Set<PersonSnapshot>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<GroupSnapshot> GroupSnapshots => Set<GroupSnapshot>();
        public DbSet<GroupRole> GroupRoles => Set<GroupRole>();
        public DbSet<GroupRoleSnapshot> GroupRoleSnapshots => Set<GroupRoleSnapshot>();
        public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
        public DbSet<DistributionList> DistributionLists => Set<DistributionList>();
        public DbSet<ReceiveRole> ReceiveRoles => Set<ReceiveRole>();
        public DbSet<SendRole> SendRoles => Set<SendRole>();
        public DbSet<SendRoleSnapshot> SendRoleSnapshots => Set<SendRoleSnapshot>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<MessageAssignment> MessageAssignments => Set<MessageAssignment>();
        public DbSet<MessageReview> MessageReviews => Set<MessageReview>();

        public DatabaseContext(IOptions<DatabaseOptions> options, ILoggerFactory loggerFactory)
        {
            this.options = options;
            this.loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseLoggerFactory(loggerFactory);
            optionsBuilder.UseMySql(options.Value.ConnectionString, ServerVersion.AutoDetect(options.Value.ConnectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Versioning strategies:
            //
            // 1. Immutable entities
            //      EntityBase contains all necessary fields to track creation and deletion.
            //      New values (e.g. deletion) might be added but no existing values are overridden.
            // 2. Mutable entities
            //      Creation and deletion are tracked by MutableEntityBase which also contains
            //      a version column and the current data. Relationships are immutable and stored
            //      in the principal entity. Each time when properties are changed, the version
            //      column is incremented and a new snapshot entity with the previous data and
            //      information about the editor and edit time is created.
            // 3. Other entities
            //      Some entities do not fit in these two patterns are handled individually


            // Mutable entity → snapshots preserve history
            var person = modelBuilder.Entity<Person>();
            person.HasKey(p => p.Id);
            ConfigureMutableEntityBase(person);

            var personSnapshot = modelBuilder.Entity<PersonSnapshot>();
            personSnapshot.HasKey(ps => new { ps.PersonId, ps.Version });
            personSnapshot.HasOne(ps => ps.Person).WithMany().HasForeignKey(ps => ps.PersonId);
            ConfigureSnapshotBase(personSnapshot);


            // Mutable entity → snapshots preserve history
            var group = modelBuilder.Entity<Group>();
            group.HasKey(g => g.Id);
            ConfigureMutableEntityBase(group);

            var groupSnapshot = modelBuilder.Entity<GroupSnapshot>();
            groupSnapshot.HasKey(gs => new { gs.GroupId, gs.Version });
            groupSnapshot.HasOne(gs => gs.Group).WithMany().HasForeignKey(gs => gs.GroupId);
            ConfigureSnapshotBase(groupSnapshot);


            // Mutable entity → snapshots preserve history
            var groupRole = modelBuilder.Entity<GroupRole>();
            groupRole.HasKey(r => r.Id);
            groupRole.HasOne(r => r.Group).WithMany().HasForeignKey(r => r.GroupId);
            ConfigureMutableEntityBase(groupRole);

            var groupRoleSnapshot = modelBuilder.Entity<GroupRoleSnapshot>();
            groupRoleSnapshot.HasKey(rs => new { rs.GroupRoleId, rs.Version });
            groupRoleSnapshot.HasOne(rs => rs.GroupRole).WithMany().HasForeignKey(rs => rs.GroupRoleId);
            ConfigureSnapshotBase(groupRoleSnapshot);


            // Immutable entity → preserves history
            var groupMember = modelBuilder.Entity<GroupMember>();
            groupMember.HasKey(m => m.Id);
            groupMember.HasOne(m => m.Person).WithMany().HasForeignKey(m => m.PersonId);
            groupMember.HasOne(m => m.GroupRole).WithMany().HasForeignKey(m => m.GroupRoleId);
            ConfigureEntityBase(groupMember);


            // Mutable entity → snapshots preserve history
            var distributionList = modelBuilder.Entity<DistributionList>();
            distributionList.HasKey(l => l.Id);
            distributionList.HasAlternateKey(l => l.Alias);
            ConfigureMutableEntityBase(distributionList);

            var distributionListSnapshot = modelBuilder.Entity<DistributionListSnapshot>();
            distributionListSnapshot.HasKey(ls => new { ls.DistributionListId, ls.Version });
            distributionListSnapshot.HasOne(ls => ls.DistributionList).WithMany().HasForeignKey(ls => ls.DistributionListId);
            ConfigureSnapshotBase(distributionListSnapshot);


            // Immutable entity → preserves history
            var receiveRole = modelBuilder.Entity<ReceiveRole>();
            receiveRole.HasKey(r => r.Id);
            receiveRole.HasOne(r => r.GroupRole).WithMany().HasForeignKey(r => r.GroupRoleId);
            receiveRole.HasOne(r => r.DistributionList).WithMany().HasForeignKey(r => r.DistributionListId);
            ConfigureEntityBase(receiveRole);


            // Mutable entity → snapshots preserve history
            var sendRole = modelBuilder.Entity<SendRole>();
            sendRole.HasKey(r => r.Id);
            sendRole.HasOne(r => r.GroupRole).WithMany().HasForeignKey(r => r.GroupRoleId);
            sendRole.HasOne(r => r.DistributionList).WithMany().HasForeignKey(r => r.DistributionListId);
            ConfigureMutableEntityBase(sendRole);

            var sendRoleSnapshot = modelBuilder.Entity<SendRoleSnapshot>();
            sendRoleSnapshot.HasKey(rs => new { rs.SendRoleId, rs.Version });
            sendRoleSnapshot.HasOne(rs => rs.SendRole).WithMany().HasForeignKey(rs => rs.SendRoleId);
            ConfigureSnapshotBase(sendRoleSnapshot);


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

        private static void ConfigureEntityBase<T>(EntityTypeBuilder<T> entity) where T : EntityBase
        {
            entity.Property(x => x.CreationTime).HasDefaultValueSql(currentTimestamp);
            entity.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.DeletedBy).WithMany().HasForeignKey(x => x.DeletedById).OnDelete(DeleteBehavior.SetNull);
        }

        private static void ConfigureMutableEntityBase<T>(EntityTypeBuilder<T> entity) where T : MutableEntityBase
        {
            entity.Property(x => x.Version).IsConcurrencyToken();
            ConfigureEntityBase(entity);
        }

        private static void ConfigureSnapshotBase<T>(EntityTypeBuilder<T> entity) where T : SnapshotBase
        {
            entity.Property(x => x.Version).ValueGeneratedNever();
            entity.Property(x => x.OverrideTime).HasDefaultValueSql(currentTimestamp);
            entity.HasOne(x => x.OverriddenBy).WithMany().HasForeignKey(x => x.OverriddenById).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
