using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.DestinationTypes;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Relativity.API;
using SystemInterface.IO;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Core.Monitoring.JobLifetime;
using kCura.IntegrationPoints.Core.Monitoring.Sinks.Aggregated;
using kCura.IntegrationPoints.Core.Serialization;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.EntityManager;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using Relativity.DataTransfer.MessageService;
using Relativity.Telemetry.APM;
using Relativity.Toggles;
using Relativity.Toggles.Providers;
using IFederatedInstanceManager = kCura.IntegrationPoints.Domain.Managers.IFederatedInstanceManager;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			IToggleProvider toggleProvider = CreateSqlServerToggleProvider(container.Resolve<IHelper>());

			container.Register(Component.For<ISerializer>().ImplementedBy<JSONSerializer>().UsingFactoryMethod(x =>
			{
				var serializer = new JSONSerializer();
				IAPILog logger = container.Resolve<IHelper>().GetLoggerFactory().GetLogger();
				return new SerializerWithLogging(serializer, logger);
			}).LifestyleSingleton());

			container.Register(Component.For<IObjectTypeRepository>().ImplementedBy<ObjectTypeRepository>().UsingFactoryMethod(x =>
			{
				IServiceContextHelper contextHelper = x.Resolve<IServiceContextHelper>();
				IHelper helper = x.Resolve<IHelper>();
				IRelativityObjectManager objectManager = x.Resolve<IRelativityObjectManager>();
				return new ObjectTypeRepository(contextHelper.WorkspaceID, helper.GetServicesManager(), helper, objectManager);
			}).LifestyleTransient());

			container.Register(Component.For<IContextContainerFactory>().ImplementedBy<ContextContainerFactory>().LifestyleTransient());
			container.Register(Component.For<ObjectTypeService>().ImplementedBy<ObjectTypeService>().LifestyleTransient());
			container.Register(Component.For<IPluginProvider>().ImplementedBy<DefaultSourcePluginProvider>().LifestyleTransient());
			container.Register(Component.For<IWindsorContainerSetup>().ImplementedBy<WindsorContainerSetup>().LifestyleSingleton());
			container.Register(Component.For<IProviderFactoryLifecycleStrategy>().ImplementedBy<ProviderFactoryLifecycleStrategy>());
			container.Register(Component.For<ProviderFactoryVendor>().ImplementedBy<ProviderFactoryVendor>().LifestyleSingleton());

			if (toggleProvider.IsEnabled<UseOldProviderCreationLogic>())
			{
				container.Register(Component.For<IDataProviderFactory>().ImplementedBy<AppDomainFactory>().LifestyleTransient());
			}
			else
			{
				container.Register(Component.For<IDataProviderFactory>().ImplementedBy<DataProviderBuilder>().LifestyleSingleton());
			}
			container.Register(Component.For<IDomainHelper>().ImplementedBy<DomainHelper>().LifestyleSingleton());
			container.Register(Component.For<IJobManager>().ImplementedBy<AgentJobManager>().LifestyleTransient());
			container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestyleTransient());
			container.Register(Component.For<IDateTimeHelper>().ImplementedBy<DateTimeUtcHelper>());

			container.Register(Component.For<IRelativityObjectManager>()
				.UsingFactoryMethod(x =>
				{
					IServiceContextHelper contextHelper = x.Resolve<IServiceContextHelper>();
					IHelper helper = x.Resolve<IHelper>();
					return new RelativityObjectManager(contextHelper.WorkspaceID,
						helper,
						new SecretStoreHelper(
							contextHelper.WorkspaceID,
							helper,
							new SecretManager(contextHelper.WorkspaceID),
							new DefaultSecretCatalogFactory()));
				}).LifestyleTransient());
			container.Register(Component.For<IRelativityObjectManagerFactory>().ImplementedBy<RelativityObjectManagerFactory>().LifestyleTransient());
			container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizer>().Named(typeof(RdoSynchronizer).AssemblyQualifiedName).LifestyleTransient());
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoEntitySynchronizer>().Named(typeof(RdoEntitySynchronizer).AssemblyQualifiedName).LifestyleTransient());
			container.Register(Component.For<IRdoSynchronizerProvider>().ImplementedBy<RdoSynchronizerProvider>().LifestyleTransient());
			container.Register(Component.For<IRelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointService, IIntegrationPointForSourceService>().ImplementedBy<IntegrationPointService>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointProfileService>().ImplementedBy<IntegrationPointProfileService>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointTypeService>().ImplementedBy<IntegrationPointTypeService>().LifestyleTransient());
			container.Register(Component.For<IGetSourceProviderRdoByIdentifier>().ImplementedBy<GetSourceProviderRdoByIdentifier>().LifestyleTransient());
			container.Register(Component.For<IBatchStatus>().ImplementedBy<BatchEmail>().LifestyleTransient());
			container.Register(Component.For<IBatchStatus>().ImplementedBy<JobHistoryBatchUpdateStatus>().LifestyleTransient());
			container.Register(Component.For<IBatchStatus>().ImplementedBy<JobLifetimeMetricBatchStatus>().LifestyleTransient());
			container.Register(Component.For<ISourceTypeFactory>().ImplementedBy<SourceTypeFactory>().LifestyleTransient());
			container.Register(Component.For<IDestinationTypeFactory>().ImplementedBy<DestinationTypeFactory>().LifestyleTransient());
			container.Register(Component.For<IResourceDbProvider>().ImplementedBy<ResourceDbProvider>().LifestyleTransient());
			container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(k => k.Resolve<IHelper>().GetServicesManager(), true));
			container.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceRepository>().ImplementedBy<KeplerWorkspaceRepository>().UsingFactoryMethod((k) => k.Resolve<IRepositoryFactory>().GetWorkspaceRepository()).LifestyleTransient());
			container.Register(Component.For<IRdoFilter>().ImplementedBy<RdoFilter>().LifestyleTransient());
			container.Register(Component.For<UserService>().ImplementedBy<UserService>().LifestyleTransient());
			container.Register(Component.For<IChoiceService>().ImplementedBy<ChoiceService>().LifestyleTransient());
			container.Register(Component.For<EntityService>().ImplementedBy<EntityService>().LifestyleTransient());
			container.Register(Component.For<ITabService>().ImplementedBy<RSAPITabService>().LifestyleTransient());
			container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<GeneralWithEntityRdoSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());
			container.Register(Component.For<IProviderFactory>().ImplementedBy<DefaultProviderFactory>().DependsOn(new { windsorContainer = container }).LifestyleTransient());
			container.Register(Component.For<IManagerQueueService>().ImplementedBy<ManagerQueueService>().LifestyleTransient());
			container.Register(Component.For<IGuidService>().ImplementedBy<DefaultGuidService>().LifestyleSingleton());
			container.Register(Component.For<IJobHistoryService>().ImplementedBy<JobHistoryService>().LifestyleTransient());
			container.Register(Component.For<IDeleteHistoryService>().ImplementedBy<DeleteHistoryService>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryErrorService>().ImplementedBy<JobHistoryErrorService>().LifestyleTransient());
			container.Register(Component.For<IJobStatusUpdater>().ImplementedBy<JobStatusUpdater>().LifestyleTransient());
			container.Register(Component.For<IJobTracker>().ImplementedBy<JobTracker>().LifestyleTransient());
			container.Register(Component.For<TaskParameterHelper>().ImplementedBy<TaskParameterHelper>().LifestyleTransient());
			container.Register(Component.For<IImportApiFactory>().ImplementedBy<ImportApiFactory>().LifestyleTransient());
			container.Register(Component.For<IImportApiFacade>().ImplementedBy<ImportApiFacade>().LifestyleTransient());
			container.Register(Component.For<IImportJobFactory>().ImplementedBy<ImportJobFactory>().LifestyleTransient());
			container.Register(Component.For<RelativityFeaturePathService>().ImplementedBy<RelativityFeaturePathService>().LifestyleTransient());

			container.Register(Component.For<IConfigFactory>().ImplementedBy<ConfigFactory>().LifestyleSingleton());
			container.Register(Component.For<ITokenProvider>().ImplementedBy<RelativityCoreTokenProvider>().LifestyleTransient());
			container.Register(Component.For<ISqlServiceFactory>().ImplementedBy<HelperConfigSqlServiceFactory>().LifestyleSingleton());
			container.Register(Component.For<IServiceManagerProvider>().ImplementedBy<ServiceManagerProvider>().LifestyleTransient());
			container.Register(Component.For<IManagerFactory>().ImplementedBy<ManagerFactory>().LifestyleTransient());

			container.Register(Component.For<ISourceWorkspaceManager>().ImplementedBy<SourceWorkspaceManager>().LifestyleTransient());
			container.Register(Component.For<ISourceJobManager>().ImplementedBy<SourceJobManager>().LifestyleTransient());
			container.Register(Component.For<IOnBehalfOfUserClaimsPrincipalFactory>().ImplementedBy<OnBehalfOfUserClaimsPrincipalFactory>().LifestyleTransient());
			container.Register(Component.For<ISavedSearchesTreeService>().ImplementedBy<SavedSearchesTreeService>().LifestyleTransient());
			container.Register(Component.For<IDirectoryTreeCreator<JsTreeItemDTO>>().ImplementedBy<DirectoryTreeCreator<JsTreeItemDTO>>().LifestyleTransient());
			container.Register(Component.For<IRelativePathDirectoryTreeCreator<JsTreeItemDTO>>().ImplementedBy<RelativePathDirectoryTreeCreator<JsTreeItemDTO>>().LifestyleTransient());
			container.Register(Component.For<IArtifactTreeCreator>().ImplementedBy<ArtifactTreeCreator>().LifestyleTransient());
			container.Register(Component.For<ISavedSearchesTreeCreator>().ImplementedBy<SavedSearchesTreeCreator>());
			container.Register(Component.For<IWorkspaceManager>().ImplementedBy<WorkspaceManager>().LifestyleTransient());
			container.Register(Component.For<IDirectory>().ImplementedBy<LongPathDirectory>().LifestyleTransient());
			container.Register(Component.For<IFile>().ImplementedBy<LongPathFile>().LifestyleTransient());
			container.Register(Component.For<IStreamFactory>().ImplementedBy<StreamFactory>().LifestyleTransient());
			container.Register(Component.For<JobStatisticsService>().ImplementedBy<JobStatisticsService>().LifestyleTransient());
			container.Register(Component.For<IFileSizesStatisticsService>().ImplementedBy<FileSizesStatisticsService>().LifestyleTransient());
			container.Register(Component.For<KeywordConverter>().ImplementedBy<KeywordConverter>().LifestyleTransient());
			container.Register(Component.For<KeywordFactory>().ImplementedBy<KeywordFactory>().LifestyleTransient());
			container.Register(Component.For<IFieldCatalogService>().ImplementedBy<FieldCatalogService>().LifestyleTransient());
			container.Register(Component.For<IImportTypeService>().ImplementedBy<ImportTypeService>().LifestyleTransient());
			container.Register(Component.For<IArtifactService>().ImplementedBy<ArtifactService>().LifestyleTransient());
			container.Register(Component.For<IProviderTypeService>().ImplementedBy<ProviderTypeService>().LifestyleTransient());
			container.Register(Component.For<IResourcePoolManager>().ImplementedBy<ResourcePoolManager>().LifestyleTransient());
			container.Register(Component.For<IResourcePoolContext>().ImplementedBy<ResourcePoolContext>().LifestyleTransient());
			container.Register(Component.For<IProcessingSourceLocationService>().ImplementedBy<ProcessingSourceLocationService>().LifestyleTransient());
			container.Register(Component.For<IDataTransferLocationService>().ImplementedBy<DataTransferLocationService>().LifestyleTransient());
			container.Register(Component.For<IDataTransferLocationServiceFactory>().ImplementedBy<DataTransferLocationServiceFactory>().DependsOn(new { container = container }).LifestyleTransient());
			container.Register(Component.For<IFolderPathReaderFactory>().ImplementedBy<FolderPathReaderFactory>().LifestyleTransient());
			container.Register(Component.For<IRsapiClientFactory>().ImplementedBy<RsapiClientFactory>());
			container.Register(Component.For<ISecretCatalogFactory>().ImplementedBy<DefaultSecretCatalogFactory>().LifestyleTransient());
			container.Register(Component.For<ISecretManagerFactory>().ImplementedBy<SecretManagerFactory>().LifestyleTransient());

			container.Register(Component.For<IIntegrationPointProviderTypeService>()
				.ImplementedBy<CachedIntegrationPointProviderTypeService>()
				.DependsOn(Dependency.OnValue<TimeSpan>(TimeSpan.FromMinutes(2))).LifestyleTransient());

			// TODO: we need to make use of an async GetDBContextAsync (pending Dan Wells' patch) -- biedrzycki: Feb 5th, 2016
			container.Register(Component.For<IToggleProvider>().Instance(toggleProvider).LifestyleTransient());

			container.Register(Component.For<IFederatedInstanceManager>().UsingFactoryMethod(k =>
			{
				IManagerFactory managerFactory = k.Resolve<IManagerFactory>();
				IContextContainerFactory contextContainerFactory = k.Resolve<IContextContainerFactory>();
				IHelper helper = k.Resolve<IHelper>();
				IContextContainer contextConainer = contextContainerFactory.CreateContextContainer(helper);
				IFederatedInstanceManager federatedInstanceManager = managerFactory.CreateFederatedInstanceManager(contextConainer);

				return federatedInstanceManager;
			}).LifestyleTransient());

			container.Register(Component.For<IServiceFactory>().ImplementedBy<ServiceFactory>().LifestyleTransient());
			container.Register(Component.For<IArtifactServiceFactory>().ImplementedBy<ArtifactServiceFactory>().LifestyleTransient());
			container.Register(Component.For<IHelperFactory>().ImplementedBy<HelperFactory>().LifestyleSingleton());
			container.Register(Component.For<IAPM>().UsingFactoryMethod(k => Client.APMClient, managedExternally: true).LifestyleTransient());
			container.Register(Component.For<IAuthProvider>().ImplementedBy<AuthProvider>().LifestyleSingleton());
			container.Register(Component.For<IOAuth2ClientFactory>().ImplementedBy<OAuth2ClientFactory>().LifestyleTransient());

			container.Register(Component.For<ICredentialProvider>().UsingFactoryMethod(kernel =>
			{
				var helper = kernel.Resolve<IHelper>();
				var authProvider = kernel.Resolve<IAuthProvider>();
				var tokenGenerator = kernel.Resolve<IAuthTokenGenerator>();

				return new TokenCredentialProvider(authProvider, tokenGenerator, helper);
			}).LifestyleTransient());

			container.Register(Component.For<ITokenProviderFactoryFactory>().ImplementedBy<TokenProviderFactoryFactory>()
				.LifestyleSingleton());

			container.Register(Component.For<IFieldService>().ImplementedBy<FieldService>().LifestyleTransient());
			container.Register(Component.For<IMetricsManagerFactory>().ImplementedBy<MetricsManagerFactory>().LifestyleSingleton());
			container.Register(Component.For<IConfig>().Instance(Config.Config.Instance).LifestyleSingleton());
			container.Register(Component.For<IMessageService>().ImplementedBy<IntegrationPointsMessageService>().LifestyleSingleton());
		}

		private SqlServerToggleProvider CreateSqlServerToggleProvider(IHelper helper)
		{
			return new SqlServerToggleProvider(() => ConnectionFactory(helper), () => AsyncConnectionFactory(helper)){ CacheEnabled = true };
		}

		private async Task<SqlConnection> AsyncConnectionFactory(IHelper helper)
		{
			Task<SqlConnection> task = Task.Run(() => ConnectionFactory(helper));
			return await task;
		}

		private SqlConnection ConnectionFactory(IHelper helper)
		{
			SqlConnection connection = helper.GetDBContext(-1).GetConnection(true);

			return connection;
		}
	}
}