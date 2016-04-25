﻿using System.Collections.Generic;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Managers.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RepositoryFactory : IRepositoryFactory
	{
		private readonly IHelper _helper;
		private IDictionary<int, ContextContainer> ContextCache { get; }

		public RepositoryFactory(IHelper helper)
		{
			_helper = helper;
			ContextCache = new Dictionary<int, ContextContainer>();
		}

		public IObjectTypeRepository GetObjectTypeRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = GetRsapiClientForWorkspace(workspaceArtifactId);
			IObjectTypeRepository repository = new RsapiObjectTypeRepository(rsapiClient);

			return repository;
		}

		public ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = GetRsapiClientForWorkspace(workspaceArtifactId);
			ISourceWorkspaceRepository repository = new RsapiSourceWorkspaceRepository(rsapiClient);

			return repository;
		}

		public IArtifactGuidRepository GetArtifactGuidRepository(int workspaceArtifactId)
		{
			BaseContext baseContext = GetBaseContextForWorkspace(workspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = new SqlArtifactGuidRepository(baseContext);

			return artifactGuidRepository;
		}

		public ISourceWorkspaceJobHistoryRepository GetSourceWorkspaceJobHistoryRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = GetRsapiClientForWorkspace(workspaceArtifactId);
			ISourceWorkspaceJobHistoryRepository repository = new SourceWorkspaceJobHistoryRepository(rsapiClient);

			return repository;
		}

		public ISourceJobRepository GetSourceJobRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = GetRsapiClientForWorkspace(workspaceArtifactId);
			ISourceJobRepository repository = new SourceJobRepository(rsapiClient);

			return repository;
		}

		public IWorkspaceRepository GetWorkspaceRepository()
		{
			IRSAPIClient rsapiClient = GetRsapiClientForWorkspace(-1);
			IWorkspaceRepository repository = new RsapiWorkspaceRepository(rsapiClient);

			return repository;
		}

		public IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId, int targetWorkspaceArtifactId)
		{
			IRSAPIClient rsapiClient = GetRsapiClientForWorkspace(sourceWorkspaceArtifactId);
			IWorkspaceRepository workspaceRepository = GetWorkspaceRepository();
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(rsapiClient, workspaceRepository, targetWorkspaceArtifactId);

			return destinationWorkspaceRepository;
		}

		public IJobHistoryRepository GetJobHistoryRepository()
		{
			IJobHistoryRepository jobHistoryRepository = new JobHistoryRepository();

			return jobHistoryRepository;
		}

		public IFieldRepository GetFieldRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = GetRsapiClientForWorkspace(workspaceArtifactId);
			BaseServiceContext baseServiceContext = GetBaseServiceContextForWorkspace(workspaceArtifactId);
			BaseContext baseContext = GetBaseContextForWorkspace(workspaceArtifactId);
			IObjectQueryManager objectQueryManager = _helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser);
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = new ObjectQueryManagerAdaptor(objectQueryManager, workspaceArtifactId, (int)ArtifactType.Field);

			IFieldRepository fieldRepository = new FieldRepository(objectQueryManagerAdaptor, baseServiceContext, baseContext, rsapiClient);

			return fieldRepository;
		}

		public ITabRepository GetTabRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = GetRsapiClientForWorkspace(workspaceArtifactId);
			ITabRepository tabRepository = new RsapiTabRepository(rsapiClient);

			return tabRepository;
		}

		public IDocumentRepository GetDocumentRepository(int workspaceArtifactId)
		{
			IObjectQueryManager objectQueryManager = _helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser);
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = new ObjectQueryManagerAdaptor(objectQueryManager, workspaceArtifactId, (int)ArtifactType.Document);

			IDocumentRepository documentRepository = new KeplerDocumentRepository(objectQueryManagerAdaptor);

			return documentRepository;
		}

		public IIntegrationPointRepository GetIntegrationPointRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = this.GetRsapiClientForWorkspace(workspaceArtifactId);
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(rsapiClient);
			return integrationPointRepository;
		}

		#region Helper Methods

		private IRSAPIClient GetRsapiClientForWorkspace(int workspaceArtifactId)
		{
			ContextContainer contexts = GetContextsForWorkspace(workspaceArtifactId);
			return contexts.RsapiClient;
		}

		private BaseContext GetBaseContextForWorkspace(int workspaceArtifactId)
		{
			ContextContainer contexts = GetContextsForWorkspace(workspaceArtifactId);
			return contexts.BaseContext;
		}

		private BaseServiceContext GetBaseServiceContextForWorkspace(int workspaceArtifactId)
		{
			ContextContainer contexts = GetContextsForWorkspace(workspaceArtifactId);
			return contexts.BaseServiceContext;
		}

		private ContextContainer GetContextsForWorkspace(int workspaceArtifactId)
		{
			ContextContainer contexts;
			if (!ContextCache.TryGetValue(workspaceArtifactId, out contexts))
			{
				IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser); // TODO: verify that this what we want or use param
				rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

				BaseServiceContext baseServiceContext = ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(workspaceArtifactId);

				BaseContext baseContext;
				if (workspaceArtifactId == -1)
				{
					baseContext =
						ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(workspaceArtifactId)
							.GetMasterDbServiceContext()
							.ThreadSafeChicagoContext;
				}
				else
				{
					baseContext = ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(workspaceArtifactId)
						.ChicagoContext
						.ThreadSafeChicagoContext;
				}

				var contextContainer = new ContextContainer()
				{
					RsapiClient = rsapiClient,
					BaseContext = baseContext,
					BaseServiceContext = baseServiceContext
				};

				ContextCache.Add(workspaceArtifactId, contextContainer);
				contexts = contextContainer;
			}

			return contexts;
		}

		#endregion

		private class ContextContainer
		{
			public IRSAPIClient RsapiClient { get; set; }
			public BaseServiceContext BaseServiceContext { get; set; }
			public BaseContext BaseContext { get; set; }
		}
	}
}