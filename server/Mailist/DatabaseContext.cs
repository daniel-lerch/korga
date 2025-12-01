using Mailist.ChurchTools.Entities;
using Mailist.EmailRelay.Entities;
using Mailist.Filters.Entities;
using Microsoft.EntityFrameworkCore;
using Mailist.EmailDelivery.Entities;

namespace Mailist;

/// <summary>
/// Central entrypoint for Korga's entire database.
/// </summary>
/// <remarks>
/// Using multiple DbContexts on a single database would come with major drawbacks:<br />
/// https://stackoverflow.com/a/11198345/7075733
/// </remarks>
public sealed class DatabaseContext : DbContext
{
    public DbSet<Person> People => Set<Person>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupRole> GroupRoles => Set<GroupRole>();
    public DbSet<GroupType> GroupTypes => Set<GroupType>();
    public DbSet<GroupStatus> GroupStatuses => Set<GroupStatus>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentMember> DepartmentMembers => Set<DepartmentMember>();
    public DbSet<Status> Status => Set<Status>();

    public DbSet<InboxEmail> InboxEmails => Set<InboxEmail>();
    public DbSet<DistributionList> DistributionLists => Set<DistributionList>();
    public DbSet<PersonFilterList> PersonFilterLists => Set<PersonFilterList>();
    public DbSet<PersonFilter> PersonFilters => Set<PersonFilter>();

    public DbSet<OutboxEmail> OutboxEmails => Set<OutboxEmail>();
    public DbSet<SentEmail> SentEmails => Set<SentEmail>();


    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CreateChurchTools(modelBuilder);

        CreateFilters(modelBuilder);

        CreateEmailRelay(modelBuilder);

        CreateEmailDelivery(modelBuilder);
    }

    private void CreateChurchTools(ModelBuilder modelBuilder)
    {
        var person = modelBuilder.Entity<Person>();
        person.HasKey(p => p.Id);
        person.HasOne(p => p.Status).WithMany().HasForeignKey(p => p.StatusId);
        person.Property(p => p.Id).ValueGeneratedNever();

        var group = modelBuilder.Entity<Group>();
        group.HasKey(x => x.Id);
        group.HasOne(x => x.GroupType).WithMany().HasForeignKey(x => x.GroupTypeId);
        group.HasOne(x => x.GroupStatus).WithMany().HasForeignKey(x => x.GroupStatusId);
        group.Property(x => x.Id).ValueGeneratedNever();

        var groupMember = modelBuilder.Entity<GroupMember>();
        groupMember.HasKey(gm => new { gm.PersonId, gm.GroupId });
        groupMember.HasOne(gm => gm.Person).WithMany().HasForeignKey(gm => gm.PersonId);
        groupMember.HasOne(gm => gm.Group).WithMany().HasForeignKey(gm => gm.GroupId);
        groupMember.HasOne(gm => gm.GroupRole).WithMany().HasForeignKey(gm => gm.GroupRoleId);
        groupMember.Property(gm => gm.GroupMemberStatus).HasConversion<int>();

        var groupType = modelBuilder.Entity<GroupType>();
        groupType.HasKey(x => x.Id);
        groupType.Property(x => x.Id).ValueGeneratedNever();

        var groupStatus = modelBuilder.Entity<GroupStatus>();
        groupStatus.HasKey(x => x.Id);
        groupStatus.Property(x => x.Id).ValueGeneratedNever();

        var groupRole = modelBuilder.Entity<GroupRole>();
        groupRole.HasKey(x => x.Id);
        groupRole.HasOne(x => x.GroupType).WithMany().HasForeignKey(x => x.GroupTypeId);
        groupRole.Property(x => x.Id).ValueGeneratedNever();

        var department = modelBuilder.Entity<Department>();
        department.HasKey(x => x.Id);
        department.Property(x => x.Id).ValueGeneratedNever();

        var departmentMember = modelBuilder.Entity<DepartmentMember>();
        departmentMember.HasKey(x => new { x.PersonId, x.DepartmentId });
        departmentMember.HasOne(x => x.Person).WithMany(p => p.Departments).HasForeignKey(x => x.PersonId);
        departmentMember.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId);

        var status = modelBuilder.Entity<Status>();
        status.HasKey(x => x.Id);
        status.Property(x => x.Id).ValueGeneratedNever();
    }

    private void CreateFilters(ModelBuilder modelBuilder)
    {
        var personFilterList = modelBuilder.Entity<PersonFilterList>();
        personFilterList.HasKey(l => l.Id);

        var personFilter = modelBuilder.Entity<PersonFilter>();
        personFilter.HasKey(f => f.Id);
        personFilter.HasOne(f => f.PersonFilterList).WithMany(l => l.Filters).HasForeignKey(f => f.PersonFilterListId);
        personFilter.HasDiscriminator(f => f.Discriminator);
        personFilter.HasIndex(f => new { f.PersonFilterListId, f.Discriminator, f.EqualityKey }).IsUnique();
        // Unique composite indices including null values are ignored by SQL databases.
        // Therefore we use a computed column which contains values for a unique index to avoid duplicate filters.
        personFilter.Property(f => f.EqualityKey).HasComputedColumnSql(@"
CONCAT(
    LPAD(HEX(IFNULL(`GroupId`, 0)), 8, '0'),
    LPAD(HEX(IFNULL(`GroupRoleId`, 0)), 8, '0'),
    LPAD(HEX(IFNULL(`GroupTypeId`, 0)), 8, '0'),
    LPAD(HEX(IFNULL(`PersonId`, 0)), 8, '0'),
    LPAD(HEX(IFNULL(`StatusId`, 0)), 8, '0'))
");

        var groupFilter = modelBuilder.Entity<GroupFilter>();
        groupFilter.HasOne(f => f.Group).WithMany().HasForeignKey(f => f.GroupId);
        groupFilter.HasOne(f => f.GroupRole).WithMany().HasForeignKey(f => f.GroupRoleId);
        groupFilter.Property(f => f.GroupRoleId).HasColumnName(nameof(GroupFilter.GroupRoleId));

        var groupTypeFilter = modelBuilder.Entity<GroupTypeFilter>();
        groupTypeFilter.HasOne(f => f.GroupType).WithMany().HasForeignKey(f => f.GroupTypeId);
        groupTypeFilter.HasOne(f => f.GroupRole).WithMany().HasForeignKey(f => f.GroupRoleId);
        groupTypeFilter.Property(f => f.GroupRoleId).HasColumnName(nameof(GroupTypeFilter.GroupRoleId));

        var statusFilter = modelBuilder.Entity<StatusFilter>();
        statusFilter.HasOne(f => f.Status).WithMany().HasForeignKey(f => f.StatusId);

        var singlePerson = modelBuilder.Entity<SinglePerson>();
        singlePerson.HasOne(f => f.Person).WithMany().HasForeignKey(f => f.PersonId);
    }

    private void CreateEmailRelay(ModelBuilder modelBuilder)
    {
        var inboxEmail = modelBuilder.Entity<InboxEmail>();
        inboxEmail.HasKey(e => e.Id);
        inboxEmail.HasOne(e => e.DistributionList).WithMany().HasForeignKey(e => e.DistributionListId);
        inboxEmail.HasIndex(e => e.UniqueId).IsUnique();
        inboxEmail.HasIndex(e => e.ProcessingCompletedTime);
        inboxEmail.Property(e => e.DownloadTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        var distributionList = modelBuilder.Entity<DistributionList>();
        distributionList.HasKey(dl => dl.Id);
        distributionList.HasAlternateKey(dl => dl.Alias);
        distributionList.Property(dl => dl.Flags).HasConversion<int>();
        distributionList.HasOne(dl => dl.PermittedRecipients).WithMany().HasForeignKey(dl => dl.PermittedRecipientsId);
    }

    private void CreateEmailDelivery(ModelBuilder modelBuilder)
    {
        var outboxEmail = modelBuilder.Entity<OutboxEmail>();
        outboxEmail.HasKey(e => e.Id);
        outboxEmail.HasOne(e => e.InboxEmail).WithMany().HasForeignKey(e => e.InboxEmailId);

        var sentEmail = modelBuilder.Entity<SentEmail>();
        sentEmail.HasKey(e => e.Id);
        sentEmail.HasOne(e => e.InboxEmail).WithMany().HasForeignKey(e => e.InboxEmailId);
        sentEmail.HasIndex(e => e.DeliveryTime);
        sentEmail.Property(e => e.Id).ValueGeneratedNever();
    }
}
