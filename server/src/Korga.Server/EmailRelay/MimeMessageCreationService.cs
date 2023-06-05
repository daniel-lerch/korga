﻿using Korga.EmailRelay.Entities;
using Korga.Server.EmailDelivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailRelay;

public class MimeMessageCreationService
{
    private readonly IOptions<EmailRelayOptions> relayOptions;
    private readonly IOptions<EmailDeliveryOptions> deliveryOptions;
    private readonly DatabaseContext database;

    public MimeMessageCreationService(IOptions<EmailRelayOptions> relayOptions, IOptions<EmailDeliveryOptions> deliveryOptions, DatabaseContext database)
    {
        this.relayOptions = relayOptions;
        this.deliveryOptions = deliveryOptions;
        this.database = database;
    }

    /// <summary>
    /// Prepends Resent headers to the original email in order to reintroduce it into the mail transport system.
    /// </summary>
    /// <param name="inboxEmail">The original email received via IMAP</param>
    /// <param name="address">The recipient to deliver the original email to</param>
    /// <returns>A complete MIME message which is ready to send</returns>
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

        headers.Insert(0, HeaderId.ResentFrom, new MailboxAddress(deliveryOptions.Value.SenderName, deliveryOptions.Value.SenderAddress).ToString());
        headers.Insert(1, HeaderId.ResentTo, address.ToString());

        return new MimeMessage(headers, body);
    }

    public async ValueTask<MimeMessage> PrepareForForwardTo(InboxEmail inboxEmail, MailboxAddress address, CancellationToken cancellationToken)
    {
        if (inboxEmail.Body == null) throw new ArgumentNullException(nameof(inboxEmail), "inboxEmail.Body must not be null");

        MimeEntity body;
        using (MemoryStream memoryStream = new(inboxEmail.Body))
            // Reading from a MemoryStream is a synchronous operation that won't be cancelled anyhow
            body = MimeEntity.Load(memoryStream, CancellationToken.None);

        MailboxAddress? from = FirstMailboxAddressOrDefault(inboxEmail.From);
        string? fromName = await FromName(from, cancellationToken);

        MimeMessage message = new();
        message.From.Add(new MailboxAddress(fromName, deliveryOptions.Value.SenderAddress));
        message.To.Add(address);
        if (from != null)
            message.ReplyTo.Add(from);
        message.Subject = inboxEmail.Subject;
        message.Body = body;
        return message;
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
            $"Bitte beachte, dass die Header kleiner als {relayOptions.Value.MaxHeaderSizeInKilobytes} KB sein müssen.");
    }

    public MimeMessage? TooBigMessage(InboxEmail inboxEmail)
    {
        int maxAttachmentSizeInMegabytes = relayOptions.Value.MaxBodySizeInKilobytes * 3 / 4 / 1024;

        return ErrorMessage(inboxEmail, $"Hallo,\r\n" +
            $"deine E-Mail mit dem Betreff {inboxEmail.Subject} an {inboxEmail.Receiver} konnte nicht zugestellt werden, weil sie zu groß war.\r\n" +
            $"Bitte beachte, dass alle Anhänge zusammen kleiner als {maxAttachmentSizeInMegabytes} MB sein müssen.");
    }

    private MimeMessage? ErrorMessage(InboxEmail inboxEmail, string message)
    {
        MailboxAddress? recipient = FirstMailboxAddressOrDefault(inboxEmail.ReplyTo)
            ?? FirstMailboxAddressOrDefault(inboxEmail.From);

        if (recipient == null) return null;

        MimeMessage errorMessage = new();
        errorMessage.From.Add(new MailboxAddress(deliveryOptions.Value.SenderName, deliveryOptions.Value.SenderAddress));
        errorMessage.To.Add(recipient);
        errorMessage.Subject = "Unzustellbar: " + inboxEmail.Subject;
        errorMessage.Body = new TextPart { Text = message };
        return errorMessage;
    }

    private async ValueTask<string?> FromName(MailboxAddress? from, CancellationToken cancellationToken)
    {
        if (from == null) return null;

        if (!string.IsNullOrWhiteSpace(from.Name)) return from.Name;

        // If the email client did not add a from name, try to get the persons name from ChurchTools
        List<string> names = await database.People.Where(p => p.Email == from.Address).Select(p => $"{p.FirstName} {p.LastName}").ToListAsync(cancellationToken);
        if (names.Count > 0) return string.Join(", ", names);

        // If no name can be determined return address like: alice@example.org <noreply@example.org>
        return from.Address;
    }

    private static MailboxAddress? FirstMailboxAddressOrDefault(string? addressList)
    {
        if (addressList == null || !InternetAddressList.TryParse(addressList, out InternetAddressList? internetAddressList))
            return null;

        foreach (InternetAddress address in internetAddressList)
        {
            // We might find no MailboxAddress in a From header if all addresses are GroupAddresses
            if (address is MailboxAddress mailboxAddress)
                return mailboxAddress;
        }

        return null;
    }
}
