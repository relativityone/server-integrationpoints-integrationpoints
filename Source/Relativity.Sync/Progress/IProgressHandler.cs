using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Progress
{
    internal interface IProgressHandler
    {
        Task<IDisposable> AttachAsync(
            int sourceWorkspaceId,
            int destinationWorkspaceId,
            int jobHistoryId,
            Guid importJobId,
            int syncConfigurationArtifactId,
            IEnumerable<int> batchIds);

        Task HandleProgressAsync();
    }
}
