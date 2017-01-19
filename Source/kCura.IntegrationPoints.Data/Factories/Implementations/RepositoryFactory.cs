using System;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
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
using Relativity.Toggles.Providers;

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
			BaseContext baseContext = GetBaseContextForWorkspace(workspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = new SqlArtifactGuidRepository(baseContext);

			return artifactGuidRepository;
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
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Field);
			IFieldRepository fieldRepository = new FieldRepository(_helper, objectQueryManagerAdaptor, workspaceArtifactId);

			return fieldRepository;
		}

		public IExtendedFieldRepository GetExtendedFieldRepository(int workspaceArtifactId)
		{
			BaseServiceContext baseServiceContext = GetBaseServiceContextForWorkspace(workspaceArtifactId);
			BaseContext baseContext = GetBaseContextForWorkspace(workspaceArtifactId);
			
			IExtendedFieldRepository extendedFieldRepository = new SqlExtendedFieldRepository(_helper, baseServiceContext, baseContext, workspaceArtifactId);	

			return extendedFieldRepository;
		}

		public IIntegrationPointRepository GetIntegrationPointRepository(int workspaceArtifactId)
		{
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(_helper, workspaceArtifactId);
			return integrationPointRepository;
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
			return new ScratchTableRepository(_helper, GetDocumentRepository(workspaceArtifactId), GetFieldRepository(workspaceArtifactId), new ResourceDbProvider(), tablePrefix, tableSuffix, workspaceArtifactId);
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

		public IErrorRepository GetErrorRepository()
		{
			IErrorRepository repository = new RsapiErrorRepository(_helper);

			return repository;
		}

		public ISavedSearchRepository GetSavedSearchRepository(int workspaceArtifactId, int savedSearchArtifactId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(workspaceArtifactId, ArtifactType.Search);
			ISavedSearchRepository repository = new SavedSearchRepository(_helper, objectQueryManagerAdaptor, workspaceArtifactId, savedSearchArtifactId, 1000);

			return repository;
		}

		public IInstanceSettingRepository GetInstanceSettingRepository()
		{
			return new InstanceSettingRepository();
		}

		public IRelativityAuditRepository GetRelativityAuditRepository(int workspaceArtifactId)
		{
			BaseServiceContext baseServiceContext = GetBaseServiceContextForWorkspace(workspaceArtifactId);
			return new RelativityAuditRepository(baseServiceContext);
		}

		public IFederatedInstanceRepository GetFederatedInstanceRepository(int artifactTypeId)
		{
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = CreateObjectQueryManagerAdaptor(-1, artifactTypeId);

			return new KeplerFederatedInstanceRepository(objectQueryManagerAdaptor);
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
			IRdoRepository rdoRepository = new RsapiRdoRepository(_helper, workspaceArtifactId);
			return rdoRepository;
		}

		public IDocumentTotalsRepository GetDocumentTotalsRepository(int workspaceArtifactId)
		{
			return new DocumentTotalsRepository(_helper, workspaceArtifactId);
		}

		public IWorkspacesRepository GetWorkspacesRepository()
        {
            IWorkspacesRepository repository = new SqlWorkspacesRepository(GetBaseContextForWorkspace(-1));
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
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = new ObjectQueryManagerAdaptor(_servicesMgr, workspaceArtifactId, artifactType);
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

        #endregion Helper Methods

        private class ContextContainer
		{
			public BaseServiceContext BaseServiceContext { get; set; }
			public BaseContext BaseContext { get; set; }
		}
	}
}