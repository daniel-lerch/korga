using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Korga.Server.Database;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventProgram> EventPrograms => Set<EventProgram>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();

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
    }
}
