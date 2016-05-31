using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Adaptors.Implementations;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Toggle;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core;
using Relativity.Toggles.Providers;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RepositoryFactory : IRepositoryFactory
	{
		private readonly IHelper _helper;
		private readonly Lazy<IExtendedRelativityToggle> _toggleProvider;

		private IDictionary<int, ContextContainer> ContextCache { get; }

		public RepositoryFactory(IHelper helper)
		{
			_helper = helper;
			ContextCache = new Dictionary<int, ContextContainer>();
			_toggleProvider = new Lazy<IExtendedRelativityToggle>(() =>
			{
				var sqlToggleProvider = new SqlServerToggleProvider(
				() => {
					SqlConnection connection = _helper.GetDBContext(-1).GetConnection(true);

					return connection;
				},
				async () => {
					Task<SqlConnection> task = Task.Run(() =>
					{
						SqlConnection connection = _helper.GetDBContext(-1).GetConnection(true);
						return connection;
					});
					return await task;
				});
				return new ExtendedRelativityToggle(sqlToggleProvider);
			});
		}

		public IArtifactGuidRepository GetArtifactGuidRepository(int workspaceArtifactId)
		{
			BaseContext baseContext = GetBaseContextForWorkspace(workspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = new SqlArtifactGuidRepository(baseContext);

			return artifactGuidRepository;
		}

		public ICodeRepository GetCodeRepository(int workspaceArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Code);
			ICodeRepository repository = new KeplerCodeRepository(objectQueryManagerAdaptor);
			return repository;
		}

		public IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId)
		{
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(_helper, sourceWorkspaceArtifactId);

			return destinationWorkspaceRepository;
		}

		public IDocumentRepository GetDocumentRepository(int workspaceArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Document);
			IDocumentRepository documentRepository = new KeplerDocumentRepository(objectQueryManagerAdaptor);
			return documentRepository;
		}

		public IFieldRepository GetFieldRepository(int workspaceArtifactId)
		{
			BaseServiceContext baseServiceContext = GetBaseServiceContextForWorkspace(workspaceArtifactId);
			BaseContext baseContext = GetBaseContextForWorkspace(workspaceArtifactId);
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Field);

			IFieldRepository fieldRepository = new FieldRepository(_helper, objectQueryManagerAdaptor, baseServiceContext, baseContext, workspaceArtifactId);
			return fieldRepository;
		}

		public IIntegrationPointRepository GetIntegrationPointRepository(int workspaceArtifactId)
		{
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(_helper, workspaceArtifactId);
			return integrationPointRepository;
		}

		public IJobHistoryRepository GetJobHistoryRepository(int workspaceArtifactId = 0)
		{
			IJobHistoryRepository jobHistoryRepository = new JobHistoryRepository(_helper, workspaceArtifactId);

			return jobHistoryRepository;
		}

		public IJobHistoryErrorRepository GetJobHistoryErrorRepository(int workspaceArtifactId)
		{
			IGenericLibrary<JobHistoryError> integrationPointLibrary = new RsapiClientLibrary<JobHistoryError>(_helper, workspaceArtifactId);
			IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> dtoTransformer = new JobHistoryErrorTransformer(_helper, workspaceArtifactId);
			IJobHistoryErrorRepository jobHistoryErrorRepository = new JobHistoryErrorRepository(_helper, integrationPointLibrary, dtoTransformer, workspaceArtifactId);
			return jobHistoryErrorRepository;
		}

		public IObjectRepository GetObjectRepository(int workspaceArtifactId, int rdoArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, rdoArtifactId);
			IObjectRepository repository = new KeplerObjectRepository(objectQueryManagerAdaptor, rdoArtifactId);
			return repository;
		}

		public IObjectTypeRepository GetObjectTypeRepository(int workspaceArtifactId)
		{
			IObjectTypeRepository repository = new RsapiObjectTypeRepository(_helper, workspaceArtifactId);

			return repository;
		}

		public IPermissionRepository GetPermissionRepository(int workspaceArtifactId)
		{
			return new PermissionRepository(_helper, workspaceArtifactId);
		}

		public IQueueRepository GetQueueRepository()
		{
			return new QueueRepository(_helper);
		}

		public IScratchTableRepository GetScratchTableRepository(int workspaceArtifactId, string tablePrefix, string tableSuffix)
		{
			return new ScratchTableRepository(_helper, _toggleProvider.Value, GetDocumentRepository(workspaceArtifactId), GetFieldRepository(workspaceArtifactId), tablePrefix, tableSuffix, workspaceArtifactId);
		}

		public ISourceJobRepository GetSourceJobRepository(int workspaceArtifactId)
		{
			ISourceJobRepository repository = new SourceJobRepository(_helper, workspaceArtifactId);

			return repository;
		}

		public ISourceProviderRepository GetSourceProviderRepository(int workspaceArtifactId)
		{
			ISourceProviderRepository sourceProviderRepository = new SourceProviderRepository(_helper, workspaceArtifactId);
			return sourceProviderRepository;
		}

		public ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId)
		{
			ISourceWorkspaceRepository repository = new RsapiSourceWorkspaceRepository(_helper, workspaceArtifactId);

			return repository;
		}

		public ISourceWorkspaceJobHistoryRepository GetSourceWorkspaceJobHistoryRepository(int workspaceArtifactId)
		{
			ISourceWorkspaceJobHistoryRepository repository = new SourceWorkspaceJobHistoryRepository(_helper, workspaceArtifactId);

			return repository;
		}

		public ITabRepository GetTabRepository(int workspaceArtifactId)
		{
			ITabRepository tabRepository = new RsapiTabRepository(_helper, workspaceArtifactId);

			return tabRepository;
		}

		public IWorkspaceRepository GetWorkspaceRepository()
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(-1, ArtifactType.Case);
			IWorkspaceRepository repository = new KeplerWorkspaceRepository(objectQueryManagerAdaptor);

			return repository;
		}

		public IErrorRepository GetErrorRepository(int workspaceArtifactId)
		{
			IErrorRepository repository = new RsapiErrorRepository(_helper, workspaceArtifactId);

			return repository;
		}

		public ISavedSearchRepository GetSavedSearchRepository(int workspaceArtifactId, int savedSearchArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Search);
			ISavedSearchRepository repository = new SavedSearchRepository(_helper, objectQueryManagerAdaptor, workspaceArtifactId, savedSearchArtifactId, 1000);

			return repository;
		}

		#region Helper Methods

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

		#endregion Helper Methods

		private class ContextContainer
		{
			public BaseServiceContext BaseServiceContext { get; set; }
			public BaseContext BaseContext { get; set; }
		}
	}
}