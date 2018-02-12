using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	public class FolderPathFromFolderTreeStrategy : FolderPathStrategyWithCache
	{
		private readonly IFolderManager _folderManager;
		private readonly int _workspaceId;

		public FolderPathFromFolderTreeStrategy(int workspaceId, IFolderManager folderManager)
		{
			_folderManager = folderManager;
			_workspaceId = workspaceId;
		}

		protected override string GetFolderPathInternal(Document document)
		{
			Task<List<FolderPath>> folderLookup = GetFolderPathFromManager(document);

			string folderPath = StripWorkspaceNameFromFolderPath(folderLookup.Result.First().FullPath);
			return folderPath;
		}

		private Task<List<FolderPath>> GetFolderPathFromManager(Document document)
		{
			Task<List<FolderPath>> folderLookup = _folderManager.GetFullPathListAsync(_workspaceId, new List<int>() { document.ParentArtifact.ArtifactID });
			folderLookup.Wait();
			if (folderLookup.Result == null || folderLookup.Result.Count == 0)
			{
				throw new Exception(
					$"Cannot find folder for document. Workspace id: {_workspaceId}. Document control number: {GetControlNumber(document)}. Document folder: {document.FolderName}");
			}

			return folderLookup;
		}

		private static FieldValue GetControlNumber(Document document)
		{
			return document[TestConstants.FieldNames.CONTROL_NUMBER];
		}

		private string StripWorkspaceNameFromFolderPath(string folderPath)
		{
			int firstSparator = folderPath.IndexOf('/');
			if (firstSparator == -1)
			{
				return string.Empty;
			}

			return folderPath.Substring(firstSparator + 1).TrimStart();
		}
	}
}