using Korga.Server.Database.ChurchTools;
using Korga.Server.Database.EmailRelay;
using Korga.Server.Database.EventRegistration;
using Korga.Server.Database.Ldap;
using Microsoft.EntityFrameworkCore;

namespace Korga.Server.Database;

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
	public DbSet<Email> Emails => Set<Email>();
	public DbSet<EmailRecipient> EmailRecipients => Set<EmailRecipient>();
	public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();

	public DbSet<PersonFilter> CTPersonFilters => Set<PersonFilter>();
	public DbSet<StatusFilterStatus> CTStatusFilterStatuses => Set<StatusFilterStatus>();

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
		var group = modelBuilder.Entity<Group>();
		group.HasKey(x => x.Id);
		group.HasOne(x => x.GroupType).WithMany().HasForeignKey(x => x.GroupTypeId);

		var groupType = modelBuilder.Entity<GroupType>();
		groupType.HasKey(x => x.Id);

		var groupRole = modelBuilder.Entity<GroupRole>();
		groupRole.HasKey(x => x.Id);
		groupRole.HasOne(x => x.GroupType).WithMany().HasForeignKey(x => x.GroupTypeId);

		var status = modelBuilder.Entity<Status>();
		status.HasKey(x => x.Id);
	}

	private void CreateEmailRelay(ModelBuilder modelBuilder)
	{
		var email = modelBuilder.Entity<Email>();
		email.HasKey(e => e.Id);
		email.Property(e => e.DownloadTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
		email.Property(e => e.DistributionListType).HasConversion<string>();

		var emailRecipient = modelBuilder.Entity<EmailRecipient>();
		emailRecipient.HasKey(e => e.Id);
		emailRecipient.HasOne(e => e.Email).WithMany(e => e.Recipients).HasForeignKey(e => e.EmailId);

		var personFilter = modelBuilder.Entity<PersonFilter>();
		personFilter.HasKey(f => f.Id);

		var groupFilter = modelBuilder.Entity<GroupFilter>();
		groupFilter.HasOne(f => f.Group).WithMany().HasForeignKey(f => f.GroupId);
		groupFilter.HasOne(f => f.GroupRole).WithMany().HasForeignKey(f => f.GroupRoleId);

		var statusFilter = modelBuilder.Entity<StatusFilter>();

		var statusFilterStatus = modelBuilder.Entity<StatusFilterStatus>();
		statusFilterStatus.HasKey(s => new { s.StatusFilterId, s.StatusId });
		statusFilterStatus.HasOne(s => s.StatusFilter).WithMany().HasForeignKey(s => s.StatusFilterId);
		statusFilterStatus.HasOne(s => s.Status).WithMany().HasForeignKey(s => s.StatusId);
	}

	private void CreateLdap(ModelBuilder modelBuilder)
	{
		var passwordReset = modelBuilder.Entity<PasswordReset>();
		passwordReset.HasKey(r => r.Token);
	}
}
