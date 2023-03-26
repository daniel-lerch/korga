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

    public async ValueTask Enqueue(string emailAddress, MimeMessage mimeMessage, CancellationToken cancellationToken)
    {
        byte[] content;

        using (MemoryStream memoryStream = new())
        {
            mimeMessage.WriteTo(memoryStream, CancellationToken.None);
            content = memoryStream.ToArray();
        }

        database.OutboxEmails.Add(new(emailAddress, content));
        await database.SaveChangesAsync(cancellationToken);
    }
}
