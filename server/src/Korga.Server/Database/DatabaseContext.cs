using Korga.Server.Configuration;
using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Korga.Server.Database
{
    public sealed partial class DatabaseContext : DbContext
    {
        private readonly IOptions<DatabaseOptions> options;
        private readonly ILoggerFactory loggerFactory;

        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventProgram> EventPrograms => Set<EventProgram>();
        public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();

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

            var @event = modelBuilder.Entity<Event>();
            @event.HasKey(e => e.Id);
            @event.Property(e => e.Start).HasPrecision(0);
            @event.Property(e => e.End).HasPrecision(0);
            @event.Property(e => e.RegistrationPeriod).HasConversion<byte>();

            var program = modelBuilder.Entity<EventProgram>();
            program.HasKey(p => p.Id);
            program.HasOne(p => p.Event).WithMany().HasForeignKey(p => p.EventId);
            program.Property(p => p.RegistrationStart).HasPrecision(0);
            program.Property(p => p.RegistrationDeadline).HasPrecision(0);

            var registration = modelBuilder.Entity<EventRegistration>();
            registration.HasKey(r => r.Id);

            var participant = modelBuilder.Entity<EventParticipant>();
            participant.HasKey(p => p.Id);
            participant.HasOne(p => p.Program).WithMany(p => p.Participants).HasForeignKey(p => p.ProgramId);
            participant.HasOne(p => p.Registration).WithMany().HasForeignKey(p => p.RegistrationId);
        }
    }
}
