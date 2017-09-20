using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Adaptors.Implementations;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RepositoryFactory : MarshalByRefObject, IRepositoryFactory
	{
		private readonly IHelper _helper;
		private readonly IServicesMgr _servicesMgr;

		public RepositoryFactory(IHelper helper, IServicesMgr servicesMgr)
		{
			_helper = helper;
			_servicesMgr = servicesMgr;
		}

		public IArtifactGuidRepository GetArtifactGuidRepository(int workspaceArtifactId)
		{
			return new KeplerArtifactGuidRepository(workspaceArtifactId, _servicesMgr);
		}

		public IArtifactTypeRepository GetArtifactTypeRepository()
		{
			BaseContext baseContext = GetBaseContextForWorkspace(-1);
			IArtifactTypeRepository artifactTypeRepository = new SqlArtifactTypeRepository(baseContext);

			return artifactTypeRepository;
		}

		public ICodeRepository GetCodeRepository(int workspaceArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Code);
			ICodeRepository repository = new KeplerCodeRepository(objectQueryManagerAdaptor);
			return repository;
		}

		public IDestinationProviderRepository GetDestinationProviderRepository(int workspaceArtifactId)
		{
			var repository = new DestinationProviderRepository(_helper, workspaceArtifactId);
			return repository;
		}

		public IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId)
		{
			var rsapiService = new RSAPIService(_helper, sourceWorkspaceArtifactId);
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(_helper, sourceWorkspaceArtifactId, rsapiService);

			return destinationWorkspaceRepository;
		}

		public IDocumentRepository GetDocumentRepository(int workspaceArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Document);
			IDocumentRepository documentRepository = new KeplerDocumentRepository(objectQueryManagerAdaptor);
			return documentRepository;
		}

		public IFieldQueryRepository GetFieldQueryRepository(int workspaceArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Field);
			IFieldQueryRepository fieldQueryRepository = new FieldQueryRepository(_helper, _servicesMgr, objectQueryManagerAdaptor, workspaceArtifactId);

			return fieldQueryRepository;
		}

		public IFieldRepository GetFieldRepository(int workspaceArtifactId)
		{
			return new FieldRepository(_servicesMgr, _helper, workspaceArtifactId);
		}

		public IJobHistoryRepository GetJobHistoryRepository(int workspaceArtifactId = 0)
		{
			IJobHistoryRepository jobHistoryRepository = new JobHistoryRepository(_helper, this, workspaceArtifactId);
			return jobHistoryRepository;
		}

		public IJobHistoryErrorRepository GetJobHistoryErrorRepository(int workspaceArtifactId)
		{
			IGenericLibrary<JobHistoryError> jobHistoryErrorLibrary = new RsapiClientLibrary<JobHistoryError>(_helper, workspaceArtifactId);
			IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> dtoTransformer = new JobHistoryErrorTransformer(_helper, workspaceArtifactId);
			IObjectTypeRepository objectTypeRepository = GetObjectTypeRepository(workspaceArtifactId);
			int objectTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistoryError));
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, objectTypeId);
			IJobHistoryErrorRepository jobHistoryErrorRepository = new JobHistoryErrorRepository(_helper, objectQueryManagerAdaptor, jobHistoryErrorLibrary, dtoTransformer, workspaceArtifactId);
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
			IObjectTypeRepository repository = new RsapiObjectTypeRepository(workspaceArtifactId, _servicesMgr, _helper);

			return repository;
		}

		public IObjectTypeRepository GetDestinationObjectTypeRepository(int workspaceArtifactId)
		{
			return new RsapiObjectTypeRepository(workspaceArtifactId, _servicesMgr, _helper);
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
			return new ScratchTableRepository(_helper, GetDocumentRepository(workspaceArtifactId), GetFieldQueryRepository(workspaceArtifactId), new ResourceDbProvider(), tablePrefix, tableSuffix, workspaceArtifactId);
		}

		public ISourceJobRepository GetSourceJobRepository(int workspaceArtifactId)
		{
			var objectTypeRepository = GetObjectTypeRepository(workspaceArtifactId);
			var fieldRepository = GetFieldRepository(workspaceArtifactId);
			IRsapiClientFactory rsapiClientFactory = new RsapiClientFactory(_helper, _servicesMgr);
			IRdoRepository rdoRepository = new RsapiRdoRepository(_helper, workspaceArtifactId, rsapiClientFactory);
			ISourceJobRepository repository = new SourceJobRepository(objectTypeRepository, fieldRepository, rdoRepository);

			return repository;
		}

		public ISourceProviderRepository GetSourceProviderRepository(int workspaceArtifactId)
		{
			ISourceProviderRepository sourceProviderRepository = new SourceProviderRepository(_helper, workspaceArtifactId);
			return sourceProviderRepository;
		}

		public ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId)
		{
			var objectTypeRepository = GetObjectTypeRepository(workspaceArtifactId);
			var fieldRepository = GetFieldRepository(workspaceArtifactId);
			IRsapiClientFactory rsapiClientFactory = new RsapiClientFactory(_helper, _servicesMgr);
			IRdoRepository rdoRepository = new RsapiRdoRepository(_helper, workspaceArtifactId, rsapiClientFactory);
			ISourceWorkspaceRepository repository = new SourceWorkspaceRepository(objectTypeRepository, fieldRepository, rdoRepository);

			return repository;
		}

		public ITabRepository GetTabRepository(int workspaceArtifactId)
		{
			ITabRepository tabRepository = new RsapiTabRepository(_servicesMgr, workspaceArtifactId);

			return tabRepository;
		}

		public IWorkspaceRepository GetWorkspaceRepository()
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(-1, ArtifactType.Case);
			IWorkspaceRepository repository = new KeplerWorkspaceRepository(_helper, _servicesMgr, objectQueryManagerAdaptor);

			return repository;
		}

		public IWorkspaceRepository GetSourceWorkspaceRepository()
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = new ObjectQueryManagerAdaptor(_helper, _helper.GetServicesManager(), -1, (int)ArtifactType.Case);
			IWorkspaceRepository repository = new KeplerWorkspaceRepository(_helper, _servicesMgr, objectQueryManagerAdaptor);

			return repository;
		}

		public IErrorRepository GetErrorRepository()
		{
			IErrorRepository repository = new RsapiErrorRepository(_helper);

			return repository;
		}

		public ISavedSearchRepository GetSavedSearchRepository(int workspaceArtifactId, int savedSearchArtifactId)
		{
			ISavedSearchRepository repository = new SavedSearchRepository(_helper, workspaceArtifactId, savedSearchArtifactId, 1000);
			return repository;
		}

		public IProductionRepository GetProductionRepository(int workspaceArtifactId)
		{
			IProductionRepository repository = new ProductionRepository(_servicesMgr);
			return repository;
		}

		public ISavedSearchQueryRepository GetSavedSearchQueryRepository(int workspaceArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Search);
			ISavedSearchQueryRepository repository = new SavedSearchQueryRepository(objectQueryManagerAdaptor);

			return repository;
		}

		public IInstanceSettingRepository GetInstanceSettingRepository()
		{
			return new KeplerInstanceSettingRepository(_helper.GetServicesManager());
		}

		public IRelativityAuditRepository GetRelativityAuditRepository(int workspaceArtifactId)
		{
			BaseServiceContext baseServiceContext = GetBaseServiceContextForWorkspace(workspaceArtifactId);
			return new RelativityAuditRepository(baseServiceContext);
		}

		public IFederatedInstanceRepository GetFederatedInstanceRepository(int artifactTypeId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(-1, artifactTypeId);

			return new KeplerFederatedInstanceRepository(_helper, objectQueryManagerAdaptor);
		}

		public IServiceUrlRepository GetServiceUrlRepository()
		{
			return new HardCodedServiceUrlRepository();
		}

		public IResourcePoolRepository GetResourcePoolRepository()
		{
			return new ResourcePoolRepository(_helper);
		}

		public IRdoRepository GetRdoRepository(int workspaceArtifactId)
		{
			IRsapiClientFactory rsapiClientFactory = new RsapiClientFactory(_helper);
			IRdoRepository rdoRepository = new RsapiRdoRepository(_helper, workspaceArtifactId, rsapiClientFactory);
			return rdoRepository;
		}

		#region Helper Methods

		private IObjectQueryManagerAdaptor CreateObjectQueryManagerAdaptor(int workspaceArtifactId, ArtifactType artifactType)
		{
			IObjectQueryManagerAdaptor adaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, (int)artifactType);
			return adaptor;
		}

		private IObjectQueryManagerAdaptor CreateObjectQueryManagerAdaptor(int workspaceArtifactId, int artifactType)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = new ObjectQueryManagerAdaptor(_helper, _servicesMgr, workspaceArtifactId, artifactType);
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
			BaseServiceContext baseServiceContext = ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactId);
			BaseContext baseContext;
			if (workspaceArtifactId == -1)
			{
				baseContext = baseServiceContext
					.GetMasterDbServiceContext()
					.ThreadSafeChicagoContext;
			}
			else
			{
				baseContext = baseServiceContext.ChicagoContext
					.ThreadSafeChicagoContext;
			}
			var contextContainer = new ContextContainer()
			{
				BaseContext = baseContext,
				BaseServiceContext = baseServiceContext
			};
			return contextContainer;
		}

		public IKeywordSearchRepository GetKeywordSearchRepository()
		{
			return new KeplerKeywordSearchRepository(_servicesMgr);
		}

		#endregion Helper Methods

		private class ContextContainer
		{
			public BaseServiceContext BaseServiceContext { get; set; }
			public BaseContext BaseContext { get; set; }
		}
	}
}