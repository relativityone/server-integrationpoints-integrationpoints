using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceProviderManager : ISourceProviderManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public SourceProviderManager(IContextContainer contextContainer)
		:this(new RepositoryFactory(contextContainer.Helper))
		{ }

		/// <summary>
		/// Unit tests should be the only external consumers of this constructor
		/// </summary>
		internal SourceProviderManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public SourceProviderDTO Read(int workspaceArtifactId, int sourceProviderArtifactId)
		{
			ISourceProviderRepository sourceProviderRepository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
			SourceProviderDTO dto = sourceProviderRepository.Read(sourceProviderArtifactId);

			return dto;
		}
	}
}