using System.Collections.Generic;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Adaptors.Implementations;
using kCura.IntegrationPoints.Data.Extensions;
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
		private IDictionary<int, ContextContainer> ContextCache { get; }

		public RepositoryFactory(IHelper helper)
		{
			_helper = helper;
			ContextCache = new Dictionary<int, ContextContainer>();
		}

		public IObjectTypeRepository GetObjectTypeRepository(int workspaceArtifactId)
		{
			IObjectTypeRepository repository = new RsapiObjectTypeRepository(_helper, workspaceArtifactId);

			return repository;
		}

		public ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId)
		{
			ISourceWorkspaceRepository repository = new RsapiSourceWorkspaceRepository(_helper, workspaceArtifactId);

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
			ISourceWorkspaceJobHistoryRepository repository = new SourceWorkspaceJobHistoryRepository(_helper, workspaceArtifactId);

			return repository;
		}

		public ISourceJobRepository GetSourceJobRepository(int workspaceArtifactId)
		{
			ISourceJobRepository repository = new SourceJobRepository(_helper, workspaceArtifactId);

			return repository;
		}

		public IWorkspaceRepository GetWorkspaceRepository()
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(-1, ArtifactType.Case);
			IWorkspaceRepository repository = new KeplerWorkspaceRepository(objectQueryManagerAdaptor);

			return repository;
		}

		public IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId)
		{
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(_helper, sourceWorkspaceArtifactId);

			return destinationWorkspaceRepository;
		}

		public IJobHistoryRepository GetJobHistoryRepository()
		{
			IJobHistoryRepository jobHistoryRepository = new JobHistoryRepository();

			return jobHistoryRepository;
		}

		public IFieldRepository GetFieldRepository(int workspaceArtifactId)
		{
			BaseServiceContext baseServiceContext = GetBaseServiceContextForWorkspace(workspaceArtifactId);
			BaseContext baseContext = GetBaseContextForWorkspace(workspaceArtifactId);
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Field);

			IFieldRepository fieldRepository = new FieldRepository(_helper, objectQueryManagerAdaptor, baseServiceContext, baseContext, workspaceArtifactId);
			return fieldRepository;
		}

		public ITabRepository GetTabRepository(int workspaceArtifactId)
		{
			ITabRepository tabRepository = new RsapiTabRepository(_helper, workspaceArtifactId);

			return tabRepository;
		}

		public IDocumentRepository GetDocumentRepository(int workspaceArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Document);
			IDocumentRepository documentRepository = new KeplerDocumentRepository(objectQueryManagerAdaptor);
			return documentRepository;
		}

		public ICodeRepository GetCodeRepository(int workspaceArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Code);
			ICodeRepository repository = new KeplerCodeRepository(objectQueryManagerAdaptor);
			return repository;
		}

		public IObjectRepository GetObjectRepository(int workspaceArtifactId, int rdoArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, rdoArtifactId);
			IObjectRepository repository = new KeplerObjectRepository(objectQueryManagerAdaptor, rdoArtifactId);
			return repository;
		}

		private IObjectQueryManagerAdaptor CreateObjectQueryManagerAdaptor(int workspaceArtifactId, ArtifactType artifactType)
		{
			IObjectQueryManagerAdaptor adaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, (int)artifactType);
			return adaptor;
		}

		private IObjectQueryManagerAdaptor CreateObjectQueryManagerAdaptor(int workspaceArtifactId, int artifactType)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = new ObjectQueryManagerAdaptor(_helper, workspaceArtifactId, artifactType);
			return objectQueryManagerAdaptor;
		}

		public IIntegrationPointRepository GetIntegrationPointRepository(int workspaceArtifactId)
		{
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(_helper, workspaceArtifactId);
			return integrationPointRepository;
		}

		#region Helper Methods

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
				BaseServiceContext baseServiceContext = ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactId);

				BaseContext baseContext;
				if (workspaceArtifactId == -1)
				{
					baseContext =
						ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactId)
							.GetMasterDbServiceContext()
							.ThreadSafeChicagoContext;
				}
				else
				{
					baseContext = ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactId)
						.ChicagoContext
						.ThreadSafeChicagoContext;
				}

				var contextContainer = new ContextContainer()
				{
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
			public BaseServiceContext BaseServiceContext { get; set; }
			public BaseContext BaseContext { get; set; }
		}
	}
}