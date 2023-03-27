using Korga.EmailRelay.Entities;
using Korga.Server.EmailDelivery;
using Microsoft.Extensions.Options;
using MimeKit;
using System.IO;

namespace Korga.Server.EmailRelay;

public class EmailRelayService
{
    private readonly IOptions<EmailDeliveryOptions> options;

    public EmailRelayService(IOptions<EmailDeliveryOptions> options)
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
}
