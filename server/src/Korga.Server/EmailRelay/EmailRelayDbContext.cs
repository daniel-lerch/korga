using Korga.Server.EmailRelay.Entities;
using Microsoft.EntityFrameworkCore;

namespace Korga.Server.EmailRelay;

public class MailingDbContext : DbContext
{
    public DbSet<Email> Emails => Set<Email>();
    public DbSet<EmailRecipient> EmailRecipients => Set<EmailRecipient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var email = modelBuilder.Entity<Email>();
        email.HasKey(e => e.Id);

        var emailRecipient = modelBuilder.Entity<EmailRecipient>();
        emailRecipient.HasKey(e => e.Id);
        emailRecipient.HasOne(e => e.Email).WithMany().HasForeignKey(e => e.EmailId);
    }
}
