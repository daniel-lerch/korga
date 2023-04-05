using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Utilities;

public interface IJobController<T>
{
    ValueTask<T?> NextPendingOrDefault(CancellationToken cancellationToken);
    ValueTask<bool> ExecuteJob(T data, CancellationToken cancellationToken);
}
