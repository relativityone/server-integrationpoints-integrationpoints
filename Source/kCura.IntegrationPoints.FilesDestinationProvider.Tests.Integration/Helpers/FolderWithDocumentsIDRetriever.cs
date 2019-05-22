using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models;
using Relativity.Services;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	public class FolderWithDocumentsIDRetriever
	{
		private readonly IFolderManager _folderManager;

		public FolderWithDocumentsIDRetriever(IFolderManager folderManager)
		{
			_folderManager = folderManager;
		}

		public void RetrieveFolderIDs(int workspaceArtifactID, IList<FolderWithDocuments> folders)
		{
			var nameToIDDictionary = new Dictionary<string, int>();

			var query = new Query();
			FolderResultSet databaseFolders = _folderManager.QueryAsync(workspaceArtifactID, query).GetAwaiter().GetResult();
			foreach (var databaseFolder in databaseFolders.Results)
			{
				FolderRef folderRef = databaseFolder.Artifact;
				nameToIDDictionary[folderRef.Name] = folderRef.ArtifactID;
			}

			foreach (var folder in folders)
			{
				if (nameToIDDictionary.ContainsKey(folder.FolderName))
				{
					folder.FolderId = nameToIDDictionary[folder.FolderName];
				}
			}
		}
	}
}
