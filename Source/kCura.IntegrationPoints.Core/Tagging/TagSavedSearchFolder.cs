using System;
using kCura.IntegrationPoints.Data.Factories;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tagging
{
	public class TagSavedSearchFolder : ITagSavedSearchFolder
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAPILog _logger;

		public TagSavedSearchFolder(IRepositoryFactory repositoryFactory, IHelper helper)
		{
			_repositoryFactory = repositoryFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<TagSavedSearchFolder>();
		}

		public int GetFolderId(int workspaceArtifactId)
		{
			try
			{
				var keywordSearchRepository = _repositoryFactory.GetKeywordSearchRepository();

				var existingFolder = keywordSearchRepository.QuerySearchContainer(workspaceArtifactId, Data.Constants.DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME);

				if (existingFolder != null)
				{
					return existingFolder.ArtifactID;
				}

				return keywordSearchRepository.CreateSearchContainerInRoot(workspaceArtifactId, Data.Constants.DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to create Saved Search container in workspace {workspaceId}.", workspaceArtifactId);
				throw;
			}
		}
	}
}