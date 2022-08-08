using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using Relativity.Services;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public class FolderWithDocumentsIdRetriever
    {
        private readonly IFolderManager _folderManager;

        public FolderWithDocumentsIdRetriever(IFolderManager folderManager)
        {
            _folderManager = folderManager;
        }

        public async Task UpdateFolderIdsAsync(int workspaceArtifactID, IList<FolderWithDocuments> folders)
        {
            IDictionary<string, int> nameToIDDictionary =
                await GetFolderNameToIDMappingAsync(workspaceArtifactID).ConfigureAwait(false);

            foreach (FolderWithDocuments folder in folders.Where(f => nameToIDDictionary.ContainsKey(f.FolderName)))
            {
                folder.FolderId = nameToIDDictionary[folder.FolderName];
            }
        }

        private async Task<IDictionary<string, int>> GetFolderNameToIDMappingAsync(int workspaceArtifactID)
        {
            var query = new Query();
            FolderResultSet folderSet = await _folderManager.QueryAsync(workspaceArtifactID, query).ConfigureAwait(false);
            IDictionary<string, int> nameToIdDictionary = folderSet.Results.ToDictionary(x => x.Artifact.Name, x => x.Artifact.ArtifactID);
            return nameToIdDictionary;
        }
    }
}
