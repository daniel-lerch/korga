﻿using Korga.EmailRelay.Entities;
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

    /// <summary>
    /// Prepends Resent headers to the original email in order to reintroduce it into the mail transport system.
    /// </summary>
    /// <param name="inboxEmail">The original email received via IMAP</param>
    /// <param name="address">The recipient to deliver the original email to</param>
    /// <returns>A complete MIME message which is ready to send</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// Forwarding emails by simply adding a Sender header fails DMARC policy checks because there is no valid DKIM signature of the original sender anymore.<br/>
    /// To avoid this problem to we resent the entire MIME message: https://www.ietf.org/rfc/rfc2822.txt (Section 3.6.6)
    /// </remarks>
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
