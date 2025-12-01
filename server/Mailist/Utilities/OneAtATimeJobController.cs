using System.Threading;
using System.Threading.Tasks;

namespace Mailist.Utilities;

public abstract class OneAtATimeJobController<T> : IJobController
{
    /// <inheritdoc/>
    public async ValueTask<bool> FetchAndExecute(CancellationToken cancellationToken)
    {
        T? data = await NextPendingOrDefault(cancellationToken);
        if (data == null) return false;
        await ExecuteJob(data, cancellationToken);
        return true;
    }
    protected abstract ValueTask<T?> NextPendingOrDefault(CancellationToken cancellationToken);
    protected abstract ValueTask ExecuteJob(T data, CancellationToken cancellationToken);
}
