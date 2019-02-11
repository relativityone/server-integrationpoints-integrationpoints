﻿using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class FolderService
	{
		private static ITestHelper Helper => new TestHelper();

		public static int CreateFolder(int workspaceArtifactId, string folderName, int? parentFolderId)
		{
			using (var folderManager = Helper.CreateAdminProxy<IFolderManager>())
			{
				var folder = new global::Relativity.Services.Folder.Folder
				{
					Name = folderName,
					ParentFolder = new FolderRef(GetParentFolderId(workspaceArtifactId, parentFolderId, folderManager))
				};

				return folderManager.CreateSingleAsync(workspaceArtifactId, folder).Result;
			}
		}

		private static int GetParentFolderId(int workspaceArtifactId, int? parentFolderId, IFolderManager folderManager)
		{
			if (parentFolderId.HasValue)
			{
				return parentFolderId.Value;
			}
			var root = folderManager.GetWorkspaceRootAsync(workspaceArtifactId).Result;
			return root.ArtifactID;
		}

		public static void DeleteUnusedFolders(int workspaceId)
		{
			using (var folderManager = Helper.CreateAdminProxy<IFolderManager>())
			{
				folderManager.DeleteUnusedFoldersAsync(workspaceId).Wait();
			}

		}
	}
}