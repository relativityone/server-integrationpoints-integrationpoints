using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public class ProviderRepository : IProviderRepository
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public ProviderRepository(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public int GetSourceProviderArtifactId(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			ISourceProviderRepository sourceProviderRepository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
			return sourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(sourceProviderGuidIdentifier);
		}

		public int GetDestinationProviderArtifactId(int workspaceArtifactId, string destinationProviderGuidIdentifier)
		{
			IDestinationProviderRepository destinationProviderRepository = _repositoryFactory.GetDestinationProviderRepository(workspaceArtifactId);
			return destinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(destinationProviderGuidIdentifier);
		}
	}
}