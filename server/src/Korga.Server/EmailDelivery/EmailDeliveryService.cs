using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailDelivery;

public class EmailDeliveryService
{
    private readonly DatabaseContext database;

    public EmailDeliveryService(DatabaseContext database)
    {
        this.database = database;
    }

    public async ValueTask<bool> Enqueue(string emailAddress, MimeMessage mimeMessage, long? inboxEmailId, CancellationToken cancellationToken)
    {
        if (inboxEmailId.HasValue && await database.OutboxEmails
                .AnyAsync(email => email.EmailAddress == emailAddress && email.InboxEmailId == inboxEmailId, cancellationToken))
            return false;

        byte[] content;

        using (MemoryStream memoryStream = new())
        {
            mimeMessage.WriteTo(memoryStream, CancellationToken.None);
            content = memoryStream.ToArray();
        }

        database.OutboxEmails.Add(new(emailAddress, content));
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }
}
