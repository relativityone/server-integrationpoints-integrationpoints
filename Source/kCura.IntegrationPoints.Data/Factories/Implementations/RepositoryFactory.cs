using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity;
using Relativity.API;
using Relativity.APIHelper.Audit;
using Relativity.Core;
using Relativity.Data;
using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API.Foundation.Repositories;
using Relativity.Services.Interfaces.ViewField;
using Relativity.Services.ResourceServer;
using ArtifactType = Relativity.ArtifactType;
using Context = kCura.Data.RowDataGateway.Context;
using IAuditRepository = kCura.IntegrationPoints.Data.Repositories.IAuditRepository;
using IErrorRepository = kCura.IntegrationPoints.Data.Repositories.IErrorRepository;
using IFieldRepository = kCura.IntegrationPoints.Data.Repositories.IFieldRepository;
using IObjectRepository = kCura.IntegrationPoints.Data.Repositories.IObjectRepository;
using QueryFieldLookup = Relativity.Data.QueryFieldLookup;

namespace kCura.IntegrationPoints.Data.Factories.Implementations
{
	public class RepositoryFactory : MarshalByRefObject, IRepositoryFactory
	{
		private readonly IHelper _helper;
		private readonly IServicesMgr _destinationServiceMgr;
		private readonly IServicesMgr _sourceServiceMgr;
		private readonly Lazy<IRelativityObjectManagerFactory> _objectManagerFactory;
		private readonly Lazy<IExternalServiceInstrumentationProvider> _instrumentationProvider;

		public RepositoryFactory(IHelper helper, IServicesMgr destinationServiceMgr)
			: this(helper, destinationServiceMgr, CreateRelativityObjectManagerFactory(helper), CreateInstrumentationProvider(helper))
		{
		}

		public RepositoryFactory(
			IHelper helper,
			IServicesMgr destinationServiceMgr,
			IRelativityObjectManagerFactory objectManagerFactory,
			IExternalServiceInstrumentationProvider instrumentationProvider)
			: this(helper, 
				destinationServiceMgr, 
				new Lazy<IRelativityObjectManagerFactory>(() => objectManagerFactory), 
				new Lazy<IExternalServiceInstrumentationProvider>(() => instrumentationProvider))
		{
		}

		private RepositoryFactory(
			IHelper helper,
			IServicesMgr destinationServiceMgr,
			Lazy<IRelativityObjectManagerFactory> objectManagerFactory,
			Lazy<IExternalServiceInstrumentationProvider> instrumentationProvider)
		{
			_helper = helper;
			_destinationServiceMgr = destinationServiceMgr;
			_sourceServiceMgr = _helper.GetServicesManager(); //TODO: it's on our wall of shame
			_objectManagerFactory = objectManagerFactory;
			_instrumentationProvider = instrumentationProvider;
		}

		private IRelativityObjectManagerFactory ObjectManagerFactory => _objectManagerFactory.Value;
		private IExternalServiceInstrumentationProvider InstrumentationProvider => _instrumentationProvider.Value;

		public IArtifactGuidRepository GetArtifactGuidRepository(int workspaceArtifactId)
		{
			return new KeplerArtifactGuidRepository(workspaceArtifactId, _destinationServiceMgr);
		}

		public IArtifactTypeRepository GetArtifactTypeRepository()
		{
			BaseContext baseContext = GetBaseContextForWorkspace(-1);
			IArtifactTypeRepository artifactTypeRepository = new SqlArtifactTypeRepository(baseContext);

			return artifactTypeRepository;
		}

		public ICodeRepository GetCodeRepository(int workspaceArtifactId)
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManager(workspaceArtifactId);
			ICodeRepository repository = new KeplerCodeRepository(relativityObjectManager);
			return repository;
		}

		public IDestinationProviderRepository GetDestinationProviderRepository(int workspaceArtifactId)
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManager(workspaceArtifactId);
			return new DestinationProviderRepository(relativityObjectManager);
		}

		public IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId)
		{
			var rsapiService = new RSAPIService(_helper, sourceWorkspaceArtifactId);
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(rsapiService);

			return destinationWorkspaceRepository;
		}

		public IDocumentRepository GetDocumentRepository(int workspaceArtifactId)
		{
			IDocumentRepository documentRepository = new KeplerDocumentRepository(ObjectManagerFactory, workspaceArtifactId);
			return documentRepository;
		}

		public IFieldQueryRepository GetFieldQueryRepository(int workspaceArtifactId)
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManagerForFederatedInstance(workspaceArtifactId);
			IFieldQueryRepository fieldQueryRepository = new FieldQueryRepository(_helper, _destinationServiceMgr, relativityObjectManager, workspaceArtifactId);

			return fieldQueryRepository;
		}

		public IFieldRepository GetFieldRepository(int workspaceArtifactId)
		{
			var foundationRepositoryFactory = new FoundationRepositoryFactory(_sourceServiceMgr, InstrumentationProvider);
			return new FieldRepository(_destinationServiceMgr, _helper, foundationRepositoryFactory, InstrumentationProvider, workspaceArtifactId);
		}

		public IJobHistoryRepository GetJobHistoryRepository(int workspaceArtifactId = 0)
		{
			IRelativityObjectManager relativityObjectManager = CreateRelativityObjectManager(workspaceArtifactId);
			IJobHistoryRepository jobHistoryRepository = new JobHistoryRepository(relativityObjectManager);
			return jobHistoryRepository;
		}

		public IJobHistoryErrorRepository GetJobHistoryErrorRepository(int workspaceArtifactId)
		{
			IJobHistoryErrorRepository jobHistoryErrorRepository = new JobHistoryErrorRepository(_helper,
				ObjectManagerFactory,
				workspaceArtifactId);
			return jobHistoryErrorRepository;
		}

		public IObjectRepository GetObjectRepository(int workspaceArtifactId, int rdoArtifactId)
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManager(workspaceArtifactId);
			IObjectRepository repository = new KeplerObjectRepository(relativityObjectManager, rdoArtifactId);
			return repository;
		}

		public IObjectTypeRepository GetObjectTypeRepository(int workspaceArtifactId)
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManager(workspaceArtifactId);
			IObjectTypeRepository repository = new ObjectTypeRepository(workspaceArtifactId, _destinationServiceMgr, _helper, relativityObjectManager);

			return repository;
		}

		public IObjectTypeRepository GetDestinationObjectTypeRepository(int workspaceArtifactId)
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManagerForFederatedInstance(workspaceArtifactId);
			return new ObjectTypeRepository(workspaceArtifactId, _destinationServiceMgr, _helper, relativityObjectManager);
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
			IObjectTypeRepository objectTypeRepository = GetObjectTypeRepository(workspaceArtifactId);
			IFieldRepository fieldRepository = GetFieldRepository(workspaceArtifactId);
			IRelativityObjectManager objectManager = CreateRelativityObjectManagerForFederatedInstance(workspaceArtifactId);
			ISourceJobRepository repository = new SourceJobRepository(objectTypeRepository, fieldRepository, objectManager);

			return repository;
		}

		public ISourceProviderRepository GetSourceProviderRepository(int workspaceArtifactId)
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManager(workspaceArtifactId);
			return new SourceProviderRepository(relativityObjectManager);
		}

		public ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId)
		{
			IObjectTypeRepository objectTypeRepository = GetObjectTypeRepository(workspaceArtifactId);
			IFieldRepository fieldRepository = GetFieldRepository(workspaceArtifactId);
			IRelativityObjectManager objectManager = CreateRelativityObjectManagerForFederatedInstance(workspaceArtifactId);
			ISourceWorkspaceRepository repository = new SourceWorkspaceRepository(_helper, objectTypeRepository, fieldRepository, objectManager);
			return repository;
		}

		public ITabRepository GetTabRepository(int workspaceArtifactId)
		{
			ITabRepository tabRepository = new RsapiTabRepository(_destinationServiceMgr, _helper, workspaceArtifactId);

			return tabRepository;
		}

		public IWorkspaceRepository GetWorkspaceRepository()
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManagerForFederatedInstance(-1);
			IWorkspaceRepository repository = new KeplerWorkspaceRepository(_helper, _destinationServiceMgr, relativityObjectManager);

			return repository;
		}

		public IWorkspaceRepository GetSourceWorkspaceRepository()
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManager(-1);
			IWorkspaceRepository repository = new KeplerWorkspaceRepository(_helper, _destinationServiceMgr, relativityObjectManager);
			return repository;
		}

		public IErrorRepository GetErrorRepository()
		{
			IErrorRepository repository = new RsapiErrorRepository(_helper);

			return repository;
		}

		public ISavedSearchRepository GetSavedSearchRepository(int workspaceArtifactId, int savedSearchArtifactId)
		{
			ISavedSearchRepository repository = new SavedSearchRepository(
				CreateRelativityObjectManager(workspaceArtifactId),
				savedSearchArtifactId,
				pageSize: 1000);
			return repository;
		}

		public IProductionRepository GetProductionRepository(int workspaceArtifactId)
		{
			IProductionRepository repository = new ProductionRepository(_destinationServiceMgr);
			return repository;
		}

		public ISavedSearchQueryRepository GetSavedSearchQueryRepository(int workspaceArtifactId)
		{
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManager(workspaceArtifactId);
			ISavedSearchQueryRepository repository = new SavedSearchQueryRepository(relativityObjectManager);

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
			IRelativityObjectManager relativityObjectManager =
				CreateRelativityObjectManager(-1);

			return new KeplerFederatedInstanceRepository(artifactTypeId, _helper, relativityObjectManager);
		}

		public IServiceUrlRepository GetServiceUrlRepository()
		{
			return new HardCodedServiceUrlRepository();
		}

		public IResourcePoolRepository GetResourcePoolRepository()
		{
			return new ResourcePoolRepository(_helper);
		}

		public IQueryFieldLookupRepository GetQueryFieldLookupRepository(int workspaceArtifactId)
		{
			string workspaceConnectionString = _helper.GetDBContext(workspaceArtifactId).GetConnection(false).ConnectionString;
			var workspaceDbContext = new Context(workspaceConnectionString);

			string masterConnectionString = kCura.Data.RowDataGateway.Config.ConnectionString;
			kCura.Data.RowDataGateway.BaseContext masterDbContext = new Context(masterConnectionString);
			int userArtifactId = ClaimsPrincipal.Current.Claims.UserArtifactID();
			int caseUserArtifactId = UserQuery.RetrieveCaseUserArtifactId(masterDbContext, userArtifactId, workspaceArtifactId);

			IQueryFieldLookup fieldLookupHelper = new QueryFieldLookup(workspaceDbContext, caseUserArtifactId, (int)ArtifactType.Document);

			IQueryFieldLookupRepository queryFieldLookupRepository = new QueryFieldLookupRepository(fieldLookupHelper, InstrumentationProvider);
			return queryFieldLookupRepository;
		}

		public IFileRepository GetFileRepository(int workspaceArtifactId)
		{
			BaseServiceContext baseServiceContext = GetBaseServiceContextForWorkspace(workspaceArtifactId);

			IFileRepository fileRepository = new FileRepository(baseServiceContext);
			return fileRepository;
		}

		public IAuditRepository GetAuditRepository(int workspaceArtifactId)
		{
			var foundationRepositoryFactory = new FoundationRepositoryFactory(_sourceServiceMgr, InstrumentationProvider);
			IExportAuditRepository exportAuditRepository = foundationRepositoryFactory.GetRepository<IExportAuditRepository>(workspaceArtifactId);
			return new AuditRepository(exportAuditRepository, InstrumentationProvider);
		}

		public IViewFieldRepository GetViewFieldRepository()
		{
			IViewFieldManager viewFieldManager = _sourceServiceMgr.CreateProxy<IViewFieldManager>(ExecutionIdentity.CurrentUser);
			return new ViewFieldRepository(viewFieldManager, InstrumentationProvider);
		}

		#region Helper Methods
		private static Lazy<IRelativityObjectManagerFactory> CreateRelativityObjectManagerFactory(IHelper helper)
		{
			return new Lazy<IRelativityObjectManagerFactory>(() => new RelativityObjectManagerFactory(helper));
		}

		private static Lazy<IExternalServiceInstrumentationProvider> CreateInstrumentationProvider(IHelper helper)
		{
			IAPILog logger = helper.GetLoggerFactory().GetLogger();
			return new Lazy<IExternalServiceInstrumentationProvider>(() => new ExternalServiceInstrumentationProviderWithoutJobContext(logger));
		}

		private IRelativityObjectManager CreateRelativityObjectManager(int workspaceArtifactId)
		{
			return ObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId);
		}

		private IRelativityObjectManager CreateRelativityObjectManagerForFederatedInstance(int workspaceArtifactId)
		{
			return ObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId, _destinationServiceMgr);
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
			return new KeplerKeywordSearchRepository(_destinationServiceMgr);
		}

		public ICaseRepository GetCaseRepository()
		{
			IResourceServerManager resourceServerManagerService = _helper.GetServicesManager()
				.CreateProxy<IResourceServerManager>(ExecutionIdentity.CurrentUser);
			return new CaseRepository(resourceServerManagerService, InstrumentationProvider);
		}

		#endregion Helper Methods

		private class ContextContainer
		{
			public BaseServiceContext BaseServiceContext { get; set; }
			public BaseContext BaseContext { get; set; }
		}
	}
}