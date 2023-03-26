using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Threading;
using System;
using System.Threading.Tasks;
using Korga.EmailDelivery.Entities;
using System.IO;
using Microsoft.Extensions.Options;

namespace Korga.Server.EmailDelivery;

public class SmtpDeliveryService : IAsyncDisposable
{
    private readonly ILogger logger;
    private readonly IOptions<EmailDeliveryOptions> options;
    private readonly DatabaseContext database;
    private SmtpClient? smtpClient;

    public SmtpDeliveryService(ILogger logger, IOptions<EmailDeliveryOptions> options, DatabaseContext database)
    {
        this.logger = logger;
        this.options = options;
        this.database = database;
    }

    public async ValueTask<bool> Send(OutboxEmail outboxEmail, CancellationToken cancellationToken)
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
            return true;
        }
        catch (SmtpCommandException ex) when (ex.StatusCode == SmtpStatusCode.MailboxBusy)
        {
            // We have to handle the case of an SMTP rate limit when multiple customers are supposed to receive a mail at the same time
            // Strato for reference only allows you to send 50 emails without delay (September 2021)
            //
            // RFC 821 defines common status code as 450 mailbox unavailable (busy or blocked for policy reasons)
            // RFC 3463 defines enhanced status code as 4.7.X for persistent transient failures caused by security or policy status

            logger.LogInformation("Mailbox busy. This is most likely caused by a temporary rate limit.");
            return false;
        }
        catch (SmtpCommandException ex) when (ex.StatusCode == SmtpStatusCode.MailboxUnavailable && ex.Message.Contains("5.7.26"))
        {
            // We have to handle emails being rejected permanently for policy violations like From headers violating DMARC policies
            //
            // RFC 7372 defines enhanced status code X.7.26 for multiple authentication failures associated to common status code 550

            logger.LogWarning("Email #{Id} has been rejected by our SMTP server for multiple authentication failures: {ErrorMessage}",
                outboxEmail.Id, ex.Message);

            // Set delivery time to prevent this email from being attempted again
            outboxEmail.DeliveryTime = DateTime.UtcNow;
            outboxEmail.ErrorMessage = ex.Message;

            await database.SaveChangesAsync(cancellationToken);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sending email #{Id} to {EmailAddress} failed",
                outboxEmail.Id, outboxEmail.EmailAddress);
            return false;
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
