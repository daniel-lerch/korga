using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Utilities;

public interface IJobController
{
    ValueTask<bool> FetchAndExecute(CancellationToken cancellationToken);
}
