using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Executors.Tagging
{
    internal interface ITaggingRepository
    {
        Task<MassUpdateResult> TagDocumentsAsync(int workspaceId, List<int> documentsIds, int destinationWorkspaceTagId, int jobHistoryId);
    }
}
