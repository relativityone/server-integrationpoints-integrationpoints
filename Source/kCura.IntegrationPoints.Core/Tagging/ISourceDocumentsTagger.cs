using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Tagging
{
    public interface ISourceDocumentsTagger
    {
        /// <summary>
        /// Mass edits/tags the Documents that were with the corresponding Destination Workspace they were pushed to.
        /// </summary>
        /// <param name="destinationWorkspaceInstanceID">Artifact ID of the DestinationWorkspace RDO instance</param>
        /// <param name="jobHistoryInstanceID">Artifact ID of the JobHistory RDO instance</param>
        /// <param name="scratchTableRepository">Scratch table repository</param>
        Task TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
            IScratchTableRepository scratchTableRepository,
            int destinationWorkspaceInstanceID,
            int jobHistoryInstanceID);
    }
}
