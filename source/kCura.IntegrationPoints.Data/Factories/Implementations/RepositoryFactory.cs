using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RepositoryFactory : IRepositoryFactory
	{
		private readonly IHelper _helper;
		private IDictionary<int, IRSAPIClient> RsapiClientCache { get; }

		public RepositoryFactory(IHelper _helper)
		{
			this._helper = _helper;
			RsapiClientCache = new Dictionary<int, IRSAPIClient>();
		}

		public ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = this.GetRsapiClientForWorkspace(workspaceArtifactId);
			ISourceWorkspaceRepository repository = new RsapiSourceWorkspaceRepository(rsapiClient);

			return repository;
		}

		public ISourceWorkspaceJobHistoryRepository GetSourceWorkspaceJobHistoryRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = this.GetRsapiClientForWorkspace(workspaceArtifactId);
			ISourceWorkspaceJobHistoryRepository repository = new SourceWorkspaceJobHistoryRepository(rsapiClient);

			return repository;
		}

		public ITargetWorkspaceJobHistoryRepository GetTargetWorkspaceJobHistoryRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = this.GetRsapiClientForWorkspace(workspaceArtifactId);
			ITargetWorkspaceJobHistoryRepository repository = new TargetWorkspaceJobHistoryRepository(rsapiClient);

			return repository;
		}

		public IWorkspaceRepository GetWorkspaceRepository()
		{
			IRSAPIClient rsapiClient = this.GetRsapiClientForWorkspace(-1);
			IWorkspaceRepository repository = new RsapiWorkspaceRepository(rsapiClient);

			return repository;
		}

		public IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId, int targetWorkspaceArtifactId)
		{
			IRSAPIClient rsapiClient = this.GetRsapiClientForWorkspace(sourceWorkspaceArtifactId);
			IWorkspaceRepository workspaceRepository = this.GetWorkspaceRepository();
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(rsapiClient, workspaceRepository, targetWorkspaceArtifactId);

			return destinationWorkspaceRepository;
		}

		private IRSAPIClient GetRsapiClientForWorkspace(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = null;
			if (!RsapiClientCache.TryGetValue(workspaceArtifactId, out rsapiClient))
			{
				rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser); // TODO: verify that this what we want or use param
				rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

				RsapiClientCache.Add(workspaceArtifactId, rsapiClient);
			}

			return rsapiClient;
		}
	}
}