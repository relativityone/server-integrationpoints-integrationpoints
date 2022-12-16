using System;
using System.Threading.Tasks;

namespace Relativity.Sync
{
    internal interface IProgressHandler
    {
        Task<IDisposable> AttachAsync(int sourceWorkspaceId, int destinationWorkspaceId, int jobHistoryId, Guid importJobId, int syncConfigurationArtifactId);

        Task HandleProgressAsync();
    }
}
