using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Korga.Server.Database;

public sealed partial class DatabaseContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventProgram> EventPrograms => Set<EventProgram>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var @event = modelBuilder.Entity<Event>();
        @event.HasKey(e => e.Id);
        @event.Property(e => e.Start).HasPrecision(0);
        @event.Property(e => e.End).HasPrecision(0);
        @event.Property(e => e.RegistrationStart).HasPrecision(0);
        @event.Property(e => e.RegistrationDeadline).HasPrecision(0);

        var program = modelBuilder.Entity<EventProgram>();
        program.HasKey(p => p.Id);
        program.HasOne(p => p.Event).WithMany().HasForeignKey(p => p.EventId);

        var registration = modelBuilder.Entity<EventRegistration>();
        registration.HasKey(r => r.Id);
        registration.HasOne(r => r.Event).WithMany().HasForeignKey(r => r.EventId);
        registration.HasAlternateKey(r => r.Token);
        registration.Property(r => r.Token).HasConversion<string>();

        var participant = modelBuilder.Entity<EventParticipant>();
        participant.HasKey(p => p.Id);
        participant.HasOne(p => p.Program).WithMany(p => p.Participants).HasForeignKey(p => p.ProgramId);
        participant.HasOne(p => p.Registration).WithMany(r => r.Participants).HasForeignKey(p => p.RegistrationId);
    }
}
