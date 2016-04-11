using System;
using System.Collections.Generic;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Authentication;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RepositoryFactory : IRepositoryFactory
	{
		private readonly IHelper _helper;
		private IDictionary<int, Tuple<IRSAPIClient, BaseContext>> ContextCache { get; }

		public RepositoryFactory(IHelper _helper)
		{
			this._helper = _helper;
			ContextCache = new Dictionary<int, Tuple<IRSAPIClient, BaseContext>>();
		}

		public ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = this.GetRsapiClientForWorkspace(workspaceArtifactId);
			IFieldHelper fieldHelper = new kCura.IntegrationPoints.Data.Helpers.FieldHelper(workspaceArtifactId);
			ISourceWorkspaceRepository repository = new RsapiSourceWorkspaceRepository(
				rsapiClient, 
				fieldHelper);

			return repository;
		}

		public IArtifactGuidRepository GetArtifactGuidRepository(int workspaceArtifactId)
		{
			BaseContext baseContext = this.GetBaseContextForWorkspace(workspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = new SqlArtifactGuidRepository(baseContext);

			return artifactGuidRepository;
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
			IFieldHelper fieldHelper = new kCura.IntegrationPoints.Data.Helpers.FieldHelper(workspaceArtifactId);
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
			Tuple<IRSAPIClient, BaseContext> contexts = this.GetContextsForWorkspace(workspaceArtifactId);
			return contexts.Item1;
		}

		private BaseContext GetBaseContextForWorkspace(int workspaceArtifactId)
		{
			Tuple<IRSAPIClient, BaseContext> contexts = this.GetContextsForWorkspace(workspaceArtifactId);
			return contexts.Item2;
		}

		private Tuple<IRSAPIClient, BaseContext> GetContextsForWorkspace(int workspaceArtifactId)
		{
			Tuple<IRSAPIClient, BaseContext> contexts = null;
			if (!ContextCache.TryGetValue(workspaceArtifactId, out contexts))
			{
				IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser); // TODO: verify that this what we want or use param
				rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

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
				var contextsTuple = new Tuple<IRSAPIClient, BaseContext>(rsapiClient, baseContext);

				ContextCache.Add(workspaceArtifactId, contextsTuple);
				contexts = contextsTuple;
			}

			return contexts;
		}
	}
}