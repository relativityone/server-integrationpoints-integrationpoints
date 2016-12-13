using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data.Factories;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public interface IRelativityProviderValidatorsFactory
	{
		FieldsMappingValidator CreateFieldsMappingValidator();

		ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName);

		SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId);

		RelativityProviderWorkspaceValidator CreateWorkspaceValidator(string prefix);

		TransferredObjectValidator CreateTransferredObjectValidator();
	}

	public class RelativityProviderValidatorsFactory : IRelativityProviderValidatorsFactory
	{
		private readonly ISerializer _serializer;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IArtifactService _artifactService;

		public RelativityProviderValidatorsFactory(ISerializer serializer, IRepositoryFactory repositoryFactory,
			IArtifactService artifactService)
		{
			_serializer = serializer;
			_repositoryFactory = repositoryFactory;
			_artifactService = artifactService;
		}

		public FieldsMappingValidator CreateFieldsMappingValidator()
		{
			return new FieldsMappingValidator(_serializer, _repositoryFactory);
		}

		public ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName)
		{
			return new ArtifactValidator(_artifactService, workspaceArtifactId, artifactTypeName);
		}

		public SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId)
		{
			return new SavedSearchValidator(_repositoryFactory.GetSavedSearchRepository(workspaceArtifactId, savedSearchArtifactId));
		}

		public RelativityProviderWorkspaceValidator CreateWorkspaceValidator(string prefix)
		{
			return new RelativityProviderWorkspaceValidator(_repositoryFactory.GetWorkspaceRepository(), prefix);
		}

		public TransferredObjectValidator CreateTransferredObjectValidator()
		{
			return new TransferredObjectValidator();
		}
	}
}