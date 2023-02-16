using Korga.ChurchTools.Entities;
using Korga.EmailRelay.Entities;
using Korga.EventRegistration.Entities;
using Korga.Ldap.Entities;
using Microsoft.EntityFrameworkCore;

namespace Korga;

/// <summary>
/// Central entrypoint for Korga's entire database.
/// </summary>
/// <remarks>
/// Using multiple DbContexts on a single database would come with major drawbacks:<br />
/// https://stackoverflow.com/a/11198345/7075733
/// </remarks>
public sealed class DatabaseContext : DbContext
{
	public DbSet<Event> Events => Set<Event>();
	public DbSet<EventProgram> EventPrograms => Set<EventProgram>();
	public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();

	public DbSet<Person> People => Set<Person>();
	public DbSet<Group> Groups => Set<Group>();
	public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
	public DbSet<GroupRole> GroupRoles => Set<GroupRole>();
	public DbSet<GroupType> GroupTypes => Set<GroupType>();
	public DbSet<Status> Status => Set<Status>();

	public DbSet<Email> Emails => Set<Email>();
	public DbSet<EmailRecipient> EmailRecipients => Set<EmailRecipient>();
	public DbSet<DistributionList> DistributionLists => Set<DistributionList>();
	public DbSet<PersonFilter> PersonFilters => Set<PersonFilter>();

	public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();


	public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		CreateEvents(modelBuilder);

		CreateChurchTools(modelBuilder);

		CreateEmailRelay(modelBuilder);

		CreateLdap(modelBuilder);
	}

	private void CreateEvents(ModelBuilder modelBuilder)
	{
		var @event = modelBuilder.Entity<Event>();
		@event.HasKey(e => e.Id);

		var program = modelBuilder.Entity<EventProgram>();
		program.HasKey(p => p.Id);
		program.HasOne(p => p.Event).WithMany().HasForeignKey(p => p.EventId);

		var participant = modelBuilder.Entity<EventParticipant>();
		participant.HasKey(p => p.Id);
		participant.HasOne(p => p.Program).WithMany(p => p.Participants).HasForeignKey(p => p.ProgramId);
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
		group.Property(x => x.Id).ValueGeneratedNever();

		var groupMember = modelBuilder.Entity<GroupMember>();
		groupMember.HasKey(gm => new { gm.PersonId, gm.GroupId });
		groupMember.HasOne(gm => gm.Person).WithMany().HasForeignKey(gm => gm.PersonId);
		groupMember.HasOne(gm => gm.Group).WithMany().HasForeignKey(gm => gm.GroupId);
		groupMember.HasOne(gm => gm.GroupRole).WithMany().HasForeignKey(gm => gm.GroupRoleId);

		var groupType = modelBuilder.Entity<GroupType>();
		groupType.HasKey(x => x.Id);
		groupType.Property(x => x.Id).ValueGeneratedNever();

		var groupRole = modelBuilder.Entity<GroupRole>();
		groupRole.HasKey(x => x.Id);
		groupRole.HasOne(x => x.GroupType).WithMany().HasForeignKey(x => x.GroupTypeId);
		groupRole.Property(x => x.Id).ValueGeneratedNever();

		var status = modelBuilder.Entity<Status>();
		status.HasKey(x => x.Id);
		status.Property(x => x.Id).ValueGeneratedNever();
	}

	private void CreateEmailRelay(ModelBuilder modelBuilder)
	{
		var email = modelBuilder.Entity<Email>();
		email.HasKey(e => e.Id);
		email.HasOne(e => e.DistributionList).WithMany().HasForeignKey(e => e.DistributionListId);
		email.Property(e => e.DownloadTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

		var emailRecipient = modelBuilder.Entity<EmailRecipient>();
		emailRecipient.HasKey(e => e.Id);
		emailRecipient.HasOne(e => e.Email).WithMany(e => e.Recipients).HasForeignKey(e => e.EmailId);

		var distributionList = modelBuilder.Entity<DistributionList>();
		distributionList.HasKey(dl => dl.Id);
		distributionList.HasAlternateKey(dl => dl.Alias);

		var personFilter = modelBuilder.Entity<PersonFilter>();
		personFilter.HasKey(f => f.Id);
		personFilter.HasOne(f => f.DistributionList).WithMany(dl => dl.Filters).HasForeignKey(f => f.DistributionListId);

		var groupFilter = modelBuilder.Entity<GroupFilter>();
		groupFilter.HasOne(f => f.Group).WithMany().HasForeignKey(f => f.GroupId);
		groupFilter.HasOne(f => f.GroupRole).WithMany().HasForeignKey(f => f.GroupRoleId);

		var statusFilter = modelBuilder.Entity<StatusFilter>();
		statusFilter.HasOne(s => s.Status).WithMany().HasForeignKey(s => s.StatusId);
	}

	private void CreateLdap(ModelBuilder modelBuilder)
	{
		var passwordReset = modelBuilder.Entity<PasswordReset>();
		passwordReset.HasKey(r => r.Token);
	}
}
