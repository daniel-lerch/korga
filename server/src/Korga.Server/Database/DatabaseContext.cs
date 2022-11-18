using Korga.Server.Database.Entities;
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


    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var @event = modelBuilder.Entity<Event>();
        @event.HasKey(e => e.Id);

        var program = modelBuilder.Entity<EventProgram>();
        program.HasKey(p => p.Id);
        program.HasOne(p => p.Event).WithMany().HasForeignKey(p => p.EventId);

        var participant = modelBuilder.Entity<EventParticipant>();
        participant.HasKey(p => p.Id);
        participant.HasOne(p => p.Program).WithMany(p => p.Participants).HasForeignKey(p => p.ProgramId);

        var email = modelBuilder.Entity<Email>();
        email.HasKey(e => e.Id);
        email.Property(e => e.DownloadTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        email.Property(e => e.DistributionListType).HasConversion<string>();

        var emailRecipient = modelBuilder.Entity<EmailRecipient>();
        emailRecipient.HasKey(e => e.Id);
        emailRecipient.HasOne(e => e.Email).WithMany(e => e.Recipients).HasForeignKey(e => e.EmailId);
    }
}
