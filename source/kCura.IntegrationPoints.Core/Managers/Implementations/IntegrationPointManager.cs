using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class IntegrationPointManager : IIntegrationPointManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public IntegrationPointManager(IContextContainer contextContainer)
		:this(new RepositoryFactory(contextContainer.Helper))
		{ }

		/// <summary>
		/// Unit tests should be the only external consumers of this constructor
		/// </summary>
		internal IntegrationPointManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public IntegrationPointDTO Read(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IIntegrationPointRepository repository = _repositoryFactory.GetIntegrationPointRepository(workspaceArtifactId);

			return repository.Read(integrationPointArtifactId);
		}
	}
}
