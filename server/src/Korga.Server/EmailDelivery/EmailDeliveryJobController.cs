using Korga.EmailDelivery.Entities;
using Korga.Server.Utilities;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailDelivery;

public class EmailDeliveryJobController : OneAtATimeJobController<OutboxEmail>, IAsyncDisposable
{
    private readonly ILogger<EmailDeliveryJobController> logger;
    private readonly IOptions<EmailDeliveryOptions> options;
    private readonly DatabaseContext database;
    private SmtpClient? smtpClient;

    public EmailDeliveryJobController(ILogger<EmailDeliveryJobController> logger, IOptions<EmailDeliveryOptions> options, DatabaseContext database)
    {
        this.logger = logger;
        this.options = options;
        this.database = database;
    }

    protected override async ValueTask<OutboxEmail?> NextPendingOrDefault(CancellationToken cancellationToken)
    {
        return await database.OutboxEmails.FirstOrDefaultAsync(email => email.DeliveryTime == default, cancellationToken);
    }

    protected override async ValueTask ExecuteJob(OutboxEmail outboxEmail, CancellationToken cancellationToken)
    {
        SmtpClient smtp = await GetConnection(cancellationToken);
        
        MimeMessage mimeMessage;

        using (MemoryStream memoryStream = new(outboxEmail.Content))
            mimeMessage = MimeMessage.Load(memoryStream, CancellationToken.None);

        try
        {
            await smtp.SendAsync(mimeMessage, cancellationToken);

            outboxEmail.DeliveryTime = DateTime.UtcNow;

            // Don't cancel this operation because messages would sent twice otherwise
            await database.SaveChangesAsync(CancellationToken.None);
            logger.LogInformation("Delivered email #{Id} to {EmailAddress}",
                outboxEmail.Id, outboxEmail.EmailAddress);
        }
        catch (SmtpCommandException ex) when (ex.StatusCode == SmtpStatusCode.MailboxBusy)
        {
            // We have to handle the case of an SMTP rate limit when multiple customers are supposed to receive a mail at the same time
            // Strato for reference only allows you to send 50 emails without delay (September 2021)
            //
            // RFC 821 defines common status code as 450 mailbox unavailable (busy or blocked for policy reasons)

            logger.LogInformation("Mailbox busy. This is most likely caused by a temporary rate limit.");
            throw new TransientFailureException("Mailbox busy. This is most likely caused by a temporary rate limit.", ex);
        }
        catch (SmtpCommandException ex) when ((int)ex.StatusCode >= 500)
        {
            // We have to handle emails being rejected permanently and must not try to send again
            //
            // RFC 3463 defines enhanced status code X.7.1 for local policy violations (Sending SPAM is not permitted)
            // RFC 7372 defines enhanced status code X.7.26 for multiple authentication failures associated to common status code 550 (DMARC violation)

            logger.LogWarning("Email #{Id} has been rejected by our SMTP server: {ErrorMessage}",
                outboxEmail.Id, ex.Message);

            // Set delivery time to prevent this email from being attempted again
            outboxEmail.DeliveryTime = DateTime.UtcNow;

            // The exception message includes the enhanced status code
            // E.g. "5.7.1 Refused by local policy. Sending of SPAM is not permitted! (B-URL)"
            outboxEmail.ErrorMessage = ex.Message;

            await database.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sending email #{Id} to {EmailAddress} failed",
                outboxEmail.Id, outboxEmail.EmailAddress);
            throw new TransientFailureException($"Sending email #{outboxEmail.Id} to {outboxEmail.EmailAddress} failed", ex);
        }
    }

    private async ValueTask<SmtpClient> GetConnection(CancellationToken cancellationToken)
    {
        SmtpClient? smtp = smtpClient;
        if (smtp == null)
        {
            smtp = new();
            await smtp.ConnectAsync(options.Value.SmtpHost, options.Value.SmtpPort, options.Value.SmtpUseSsl, cancellationToken);
            await smtp.AuthenticateAsync(options.Value.SmtpUsername, options.Value.SmtpPassword, cancellationToken);
            smtpClient = smtp;
        }
        return smtp;
    }

    public async ValueTask DisposeAsync()
    {
        if (smtpClient != null)
        {
            await smtpClient.DisconnectAsync(quit: true);
            smtpClient.Dispose();
            smtpClient = null;
        }
    }
}
