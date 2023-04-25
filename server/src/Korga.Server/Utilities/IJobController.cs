using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Utilities;

public interface IJobController
{
    /// <summary>
    /// Fetches jobs and executes them
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a job has been executed; <see langword="false"/> if the job queue was empty.
    /// </returns>
    ValueTask<bool> FetchAndExecute(CancellationToken cancellationToken);
}
