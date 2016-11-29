using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data.Factories;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public interface IRelativityProviderValidatorsFactory
	{
		FieldsMappingValidator CreateFieldsMappingValidator();

		FolderValidator CreateFolderValidator(int workspaceArtifactId, int folderArtifactId);

		SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId);

		WorkspaceValidator CreateWorkspaceValidator(string prefix);
	}

	public class RelativityProviderValidatorsFactory : IRelativityProviderValidatorsFactory
	{
		private readonly ISerializer _serializer;
		private readonly IRepositoryFactory _repositoryFactory;

		public RelativityProviderValidatorsFactory(ISerializer serializer, IRepositoryFactory repositoryFactory)
		{
			_serializer = serializer;
			_repositoryFactory = repositoryFactory;
		}

		public FieldsMappingValidator CreateFieldsMappingValidator()
		{
			return new FieldsMappingValidator(_serializer, _repositoryFactory);
		}

		public FolderValidator CreateFolderValidator(int workspaceArtifactId, int folderArtifactId)
		{
			return new FolderValidator();
		}

		public SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId)
		{
			return new SavedSearchValidator(_repositoryFactory.GetSavedSearchRepository(workspaceArtifactId, savedSearchArtifactId));
		}

		public WorkspaceValidator CreateWorkspaceValidator(string prefix)
		{
			return new WorkspaceValidator(_repositoryFactory.GetWorkspaceRepository(), prefix);
		}
	}
}