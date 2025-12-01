using MimeKit;

namespace Mailist.Extensions;

public static class HeaderListExtensions
{
    public static string? GetReceiver(this HeaderList headers)
    {
        // 1. Try to get receiver from Received header
        string? receivedHeader = headers[HeaderId.Received];
        if (receivedHeader != null)
        {
            string prefix = "for <";
            string suffix = ">;";
            int prefixIdx = receivedHeader.IndexOf(prefix);
            if (prefixIdx != -1)
            {
                int endIdx = receivedHeader.IndexOf(suffix);
                if (endIdx != -1)
                {
                    int startIdx = prefixIdx + prefix.Length;
                    return receivedHeader[startIdx..endIdx];
                }
            }
        }

        // 2. Try to get receiver from Envelope-To or X-Envelope-To headers
        string? envelopeTo = headers["Envelope-To"] ?? headers["X-Envelope-To"];
        if (envelopeTo != null)
        {
            int prefixIdx = envelopeTo.IndexOf('<');
            if (prefixIdx != -1)
            {
                int endIdx = envelopeTo.IndexOf('>');
                if (endIdx != -1)
                {
                    return envelopeTo[(prefixIdx + 1)..endIdx];
                }
            }
        }

        return null;
    }
}
