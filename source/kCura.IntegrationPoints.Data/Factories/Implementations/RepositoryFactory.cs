﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core;
using FieldHelper = kCura.IntegrationPoints.Data.Helpers.FieldHelper;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RepositoryFactory : IRepositoryFactory
	{
		private readonly IHelper _helper;
		private IDictionary<int, IRSAPIClient> RsapiClientCache { get; }

		private readonly BaseServiceContext _baseContext;

		public RepositoryFactory(IHelper _helper)
		{
			this._helper = _helper;
			RsapiClientCache = new Dictionary<int, IRSAPIClient>();

		}

		public ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = this.GetRsapiClientForWorkspace(workspaceArtifactId);
			IFieldHelper fieldHelper = new FieldHelper(workspaceArtifactId);
			ISourceWorkspaceRepository repository = new RsapiSourceWorkspaceRepository(rsapiClient, fieldHelper);
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
			IFieldHelper fieldHelper = new FieldHelper(workspaceArtifactId);
			ITargetWorkspaceJobHistoryRepository repository = new TargetWorkspaceJobHistoryRepository(rsapiClient, fieldHelper);
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

		public IJobHistoryRepository GetJobHistoryRepository()
		{
			IJobHistoryRepository jobHistoryRepository = new JobHistoryRepository();

			return jobHistoryRepository;
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