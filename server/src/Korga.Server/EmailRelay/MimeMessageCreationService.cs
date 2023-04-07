using Korga.EmailRelay.Entities;
using Korga.Server.EmailDelivery;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.IO;

namespace Korga.Server.EmailRelay;

public class MimeMessageCreationService
{
    private readonly IOptions<EmailDeliveryOptions> options;

    public MimeMessageCreationService(IOptions<EmailDeliveryOptions> options)
    {
        this.options = options;
    }

    public MimeMessage PrepareForResentTo(InboxEmail inboxEmail, MailboxAddress address)
    {
        if (inboxEmail.Header == null) throw new ArgumentNullException(nameof(inboxEmail), "inboxEmail.Header must not be null");
        if (inboxEmail.Body == null) throw new ArgumentNullException(nameof(inboxEmail), "inboxEmail.Body must not be null");

        HeaderList headers;
        using (MemoryStream memoryStream = new(inboxEmail.Header))
            headers = HeaderList.Load(memoryStream);

        MimeEntity body;
        using (MemoryStream memoryStream = new(inboxEmail.Body))
            body = MimeEntity.Load(memoryStream);

        headers.Insert(0, HeaderId.ResentFrom, new MailboxAddress(options.Value.SenderName, options.Value.SenderAddress).ToString());
        headers.Insert(1, HeaderId.ResentTo, address.ToString());

        return new MimeMessage(headers, body);
    }

    public MimeMessage? InvalidServerConfiguration(InboxEmail inboxEmail)
    {
        return ErrorMessage(inboxEmail, $"Hallo,\r\n" +
            $"deine E-Mail mit dem Betreff {inboxEmail.Subject} an {inboxEmail.Receiver} konnte nicht zugestellt werden.\r\n" +
            $"Der E-Mail-Server hat die Empfängeradresse weder im Received Header, noch im Envelope-To oder X-Envelope-To Header mitgesendet.\r\n" +
            $"Bitte wende dich an den Administrator.");
    }

    public MimeMessage? InvalidAlias(InboxEmail inboxEmail)
    {
        return ErrorMessage(inboxEmail, $"Hallo,\r\n" +
            $"deine E-Mail mit dem Betreff {inboxEmail.Subject} an {inboxEmail.Receiver} konnte nicht zugestellt werden.\r\n" +
            $"Die E-Mail-Adresse {inboxEmail.Receiver} existiert nicht.");
    }

    public MimeMessage? TooManyHeaders(InboxEmail inboxEmail)
    {
        return ErrorMessage(inboxEmail, $"Hallo,\r\n" +
            $"deine E-Mail mit dem Betreff {inboxEmail.Subject} an {inboxEmail.Receiver} konnte nicht zugestellt werden, weil sie zu viele Header enthielt.\r\n" +
            $"Bitte beachte, dass die Header kleiner als 64 KB sein müssen.");
    }

    public MimeMessage? TooBigMessage(InboxEmail inboxEmail)
    {
        return ErrorMessage(inboxEmail, $"Hallo,\r\n" +
            $"deine E-Mail mit dem Betreff {inboxEmail.Subject} an {inboxEmail.Receiver} konnte nicht zugestellt werden, weil sie zu groß war.\r\n" +
            $"Bitte beachte, dass alle Anhänge zusammen kleiner als 9 MB sein müssen.");
    }

    private MimeMessage? ErrorMessage(InboxEmail inboxEmail, string message)
    {
        InternetAddress recipient;

        if (inboxEmail.ReplyTo != null && MailboxAddress.TryParse(inboxEmail.ReplyTo, out MailboxAddress replyTo))
        {
            recipient = replyTo;
        }
        else if (InternetAddressList.TryParse(inboxEmail.From, out InternetAddressList? originalFrom) && originalFrom.Count > 0)
        {
            recipient = originalFrom[0];
        }
        else
        {
            return null;
        }

        MimeMessage errorMessage = new();
        errorMessage.From.Add(new MailboxAddress(options.Value.SenderName, options.Value.SenderAddress));
        errorMessage.To.Add(recipient);
        errorMessage.Subject = "Unzustellbar: " + inboxEmail.Subject;
        errorMessage.Body = new TextPart { Text = message };
        return errorMessage;
    }
}
