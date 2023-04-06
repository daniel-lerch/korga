using Nito.AsyncEx;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Korga.Server.Extensions;

public static class AsyncExtensions
{
    // Inspired by https://github.com/StephenCleary/AsyncEx/issues/212#issuecomment-653765593
    public static async Task<bool> WaitAsync(this AsyncAutoResetEvent mEvent, TimeSpan timeout, CancellationToken token = default)
    {
        using var timeOut = new CancellationTokenSource(timeout);
        using var combined = CancellationTokenSource.CreateLinkedTokenSource(timeOut.Token, token);

        try
        {
            await mEvent.WaitAsync(combined.Token).ConfigureAwait(false);
            return true;
        }
        // Don't catch the OperationCanceledException from external Token
        catch (OperationCanceledException) when (!token.IsCancellationRequested) 
        {
            return false; //Here the OperationCanceledException was raised by Timeout
        }
    }
}
