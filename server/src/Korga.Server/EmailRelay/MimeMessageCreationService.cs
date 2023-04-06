using Korga.EmailRelay.Entities;
using Korga.Server.EmailDelivery;
using Microsoft.Extensions.Options;
using MimeKit;
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
        if (InternetAddressList.TryParse(inboxEmail.From, out InternetAddressList? originalFrom))
        {
            MimeMessage errorMessage = new();
            errorMessage.From.Add(new MailboxAddress(options.Value.SenderName, options.Value.SenderAddress));
            errorMessage.To.Add(originalFrom[0]);
            errorMessage.Subject = "Unzustellbar: " + inboxEmail.Subject;
            errorMessage.Body = new TextPart { Text = $"Hallo,\r\ndeine E-Mail mit dem Betreff {inboxEmail.Subject} an {inboxEmail.Receiver} konnte nicht zugestellt werden. Der E-Mail-Server hat die Empfängeradresse weder im Received Header, noch im Envelope-To oder X-Envelope-To Header mitgesendet. Bitte wende dich an den Administrator." };
            return errorMessage;
        }
        else
        {
            return null;
        }
    }

    public MimeMessage? InvalidAlias(InboxEmail inboxEmail)
    {
        if (InternetAddressList.TryParse(inboxEmail.From, out InternetAddressList? originalFrom))
        {
            MimeMessage errorMessage = new();
            errorMessage.From.Add(new MailboxAddress(options.Value.SenderName, options.Value.SenderAddress));
            errorMessage.To.Add(originalFrom[0]);
            errorMessage.Subject = "Unzustellbar: " + inboxEmail.Subject;
            errorMessage.Body = new TextPart { Text = $"Hallo,\r\ndeine E-Mail mit dem Betreff {inboxEmail.Subject} an {inboxEmail.Receiver} konnte nicht zugestellt werden. Die E-Mail-Adresse {inboxEmail.Receiver} existiert nicht." };
            return errorMessage;
        }
        else
        {
            return null;
        }
    }
}
