using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Domain;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.CustodianManager;
using kCura.IntegrationPoints.Core.Services.DestinationTypes;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.CustodianManager;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;
using SystemInterface.IO;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.ScheduleQueue.Core.Data;
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
			var guid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

			container.Register(Component.For<ISerializer>().ImplementedBy<JSONSerializer>().LifestyleSingleton());
			container.Register(Component.For<IIntegrationPointSerializer>().ImplementedBy<IntegrationPointSerializer>().LifestyleSingleton());
			container.Register(Component.For<IObjectTypeRepository>().ImplementedBy<RsapiObjectTypeRepository>().UsingFactoryMethod(x =>
			{
				IServiceContextHelper contextHelper = x.Resolve<IServiceContextHelper>();
				IHelper helper = x.Resolve<IHelper>();
				return new RsapiObjectTypeRepository(contextHelper.WorkspaceID, helper.GetServicesManager(), helper);
			}).LifestyleTransient());

			container.Register(Component.For<IContextContainerFactory>().ImplementedBy<ContextContainerFactory>().LifestyleTransient());
			container.Register(Component.For<IErrorService>().ImplementedBy<ErrorService>().Named("ErrorService").LifestyleTransient());
			container.Register(Component.For<ObjectTypeService>().ImplementedBy<ObjectTypeService>().LifestyleTransient());
			container.Register(Component.For<ISourcePluginProvider>().ImplementedBy<DefaultSourcePluginProvider>().LifestyleTransient());
			container.Register(Component.For<IDataProviderFactory>().ImplementedBy<AppDomainFactory>().LifestyleTransient());
			container.Register(Component.For<DomainHelper>().ImplementedBy<DomainHelper>().LifestyleSingleton());
			container.Register(Component.For<IJobManager>().ImplementedBy<AgentJobManager>().LifestyleTransient());
			container.Register(Component.For<IJobServiceDataProvider>().ImplementedBy<JobServiceDataProvider>().LifestyleTransient());
			container.Register(Component.For<IJobService>().ImplementedBy<JobService>().LifestyleTransient());
			container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestyleTransient());
			container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());
			container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().DependsOn(Dependency.OnValue<Guid>(guid)).LifestyleTransient());
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizer>().Named(typeof(RdoSynchronizer).AssemblyQualifiedName).LifestyleTransient());
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoCustodianSynchronizer>().Named(typeof(RdoCustodianSynchronizer).AssemblyQualifiedName).LifestyleTransient());
			container.Register(Component.For<IRdoSynchronizerProvider>().ImplementedBy<RdoSynchronizerProvider>().LifestyleTransient());
			container.Register(Component.For<IRelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointService>().ImplementedBy<IntegrationPointService>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointProfileService>().ImplementedBy<IntegrationPointProfileService>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointTypeService>().ImplementedBy<IntegrationPointTypeService>().LifestyleTransient());
			container.Register(Component.For<IGetSourceProviderRdoByIdentifier>().ImplementedBy<GetSourceProviderRdoByIdentifier>().LifestyleTransient());
			container.Register(Component.For<IBatchStatus>().ImplementedBy<BatchEmail>().LifestyleTransient());
			container.Register(Component.For<IBatchStatus>().ImplementedBy<JobHistoryBatchUpdateStatus>().LifestyleTransient());
			container.Register(Component.For<ISourceTypeFactory>().ImplementedBy<SourceTypeFactory>().LifestyleTransient());
			container.Register(Component.For<IDestinationTypeFactory>().ImplementedBy<DestinationTypeFactory>().LifestyleTransient());
			container.Register(Component.For<IResourceDbProvider>().ImplementedBy<ResourceDbProvider>().LifestyleTransient());
		    container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(k => k.Resolve<IHelper>().GetServicesManager(), true));
            container.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceRepository>().ImplementedBy<KeplerWorkspaceRepository>().UsingFactoryMethod((k) => k.Resolve<IRepositoryFactory>().GetWorkspaceRepository()).LifestyleTransient());
			container.Register(Component.For<IRdoFilter>().ImplementedBy<RdoFilter>().LifestyleTransient());
			container.Register(Component.For<UserService>().ImplementedBy<UserService>().LifestyleTransient());
			container.Register(Component.For<IChoiceService>().ImplementedBy<ChoiceService>().LifestyleTransient());
			container.Register(Component.For<CustodianService>().ImplementedBy<CustodianService>().LifestyleTransient());
			container.Register(Component.For<ITabService>().ImplementedBy<RSAPITabService>().LifestyleTransient());
			container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<GeneralWithCustodianRdoSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());
			container.Register(Component.For<IProviderFactory>().ImplementedBy<DefaultProviderFactory>().DependsOn(new { windsorContainer = container }).LifestyleTransient());
			container.Register(Component.For<IManagerQueueService>().ImplementedBy<ManagerQueueService>().LifestyleTransient());
			container.Register(Component.For<IGuidService>().ImplementedBy<DefaultGuidService>().LifestyleSingleton());
			container.Register(Component.For<IJobHistoryService>().ImplementedBy<JobHistoryService>().LifestyleTransient());
			container.Register(Component.For<IUnlinkedJobHistoryService>().ImplementedBy<UnlinkedJobHistoryService>().LifestyleTransient());
			container.Register(Component.For<IDeleteHistoryErrorService>().ImplementedBy<DeleteHistoryErrorService>().LifestyleTransient());
			container.Register(Component.For<IDeleteHistoryService>().ImplementedBy<DeleteHistoryService>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryErrorService>().ImplementedBy<JobHistoryErrorService>().LifestyleTransient());
			container.Register(Component.For<IJobStatusUpdater>().ImplementedBy<JobStatusUpdater>().LifestyleTransient());
			container.Register(Component.For<IJobTracker>().ImplementedBy<JobTracker>().LifestyleTransient());
			container.Register(Component.For<TaskParameterHelper>().ImplementedBy<TaskParameterHelper>().LifestyleTransient());
			container.Register(Component.For<IImportApiFactory>().ImplementedBy<ImportApiFactory>().LifestyleTransient());
			container.Register(Component.For<IImportJobFactory>().ImplementedBy<ImportJobFactory>().LifestyleTransient());
			container.Register(Component.For<RelativityFeaturePathService>().ImplementedBy<RelativityFeaturePathService>().LifestyleTransient());

			container.Register(Component.For<IConfigFactory>().ImplementedBy<ConfigFactory>().LifestyleSingleton());
			container.Register(Component.For<ITokenProvider>().ImplementedBy<RelativityCoreTokenProvider>().LifestyleTransient());
			container.Register(Component.For<ISqlServiceFactory>().ImplementedBy<HelperConfigSqlServiceFactory>().LifestyleSingleton());
			container.Register(Component.For<IServiceManagerProvider>().ImplementedBy<ServiceManagerProvider>().LifestyleTransient());
			container.Register(Component.For<IManagerFactory>().ImplementedBy<ManagerFactory>().LifestyleTransient());

			container.Register(Component.For<IEncryptionManager>().ImplementedBy<DefaultEncryptionManager>().LifestyleSingleton());
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
			container.Register(Component.For<KeywordConverter>().ImplementedBy<KeywordConverter>().LifestyleTransient());
			container.Register(Component.For<KeywordFactory>().ImplementedBy<KeywordFactory>().LifestyleTransient());
			container.Register(Component.For<IFieldCatalogService>().ImplementedBy<FieldCatalogService>().LifestyleTransient());
			container.Register(Component.For<IImportTypeService>().ImplementedBy<ImportTypeService>().LifestyleTransient());
			container.Register(Component.For<IArtifactService>().ImplementedBy<ArtifactService>().LifestyleTransient());
			container.Register(Component.For<IProviderTypeService>().ImplementedBy<ProviderTypeService>().LifestyleTransient());
			container.Register(Component.For<IDataTransferLocationService>().ImplementedBy<DataTransferLocationService>().LifestyleTransient());
			container.Register(Component.For<IResourcePoolManager>().ImplementedBy<ResourcePoolManager>().LifestyleTransient());
			container.Register(Component.For<IDataTransferLocationServiceFactory>().ImplementedBy<DataTransferLocationServiceFactory>().DependsOn(new { container = container }).LifestyleTransient());
			container.Register(Component.For<IFolderPathReaderFactory>().ImplementedBy<FolderPathReaderFactory>().LifestyleTransient());
			container.Register(Component.For<IUnfinishedJobService>().ImplementedBy<UnfinishedJobService>().LifestyleTransient());
			container.Register(Component.For<IRSAPIServiceFactory>().ImplementedBy<RSAPIServiceFactory>().LifestyleTransient());
			container.Register(Component.For<IRunningJobService>().ImplementedBy<RunningJobService>().LifestyleTransient());
			container.Register(Component.For<ISecretCatalogFactory>().ImplementedBy<DefaultSecretCatalogFactory>().LifestyleTransient());
			container.Register(Component.For<ISecretManagerFactory>().ImplementedBy<SecretManagerFactory>().LifestyleTransient());

			// TODO: we need to make use of an async GetDBContextAsync (pending Dan Wells' patch) -- biedrzycki: Feb 5th, 2016
			container.Register(Component.For<IToggleProvider>().Instance(new SqlServerToggleProvider(
				() =>
				{
					SqlConnection connection = container.Resolve<IHelper>().GetDBContext(-1).GetConnection(true);

					return connection;
				},
				async () =>
				{
					Task<SqlConnection> task = Task.Run(() =>
					{
						SqlConnection connection = container.Resolve<IHelper>().GetDBContext(-1).GetConnection(true);
						return connection;
					});

					return await task;
				})).LifestyleTransient());

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

			container.Register(Component.For<IRdoRepository>().ImplementedBy<RsapiRdoRepository>().UsingFactoryMethod(
				kernel =>
				{
					var helper = kernel.Resolve<IHelper>();
					var contextHelper = kernel.Resolve<IServiceContextHelper>();
					var rsapiClientFactory = kernel.Resolve<IRsapiClientFactory>();

					return new RsapiRdoRepository(helper, contextHelper.WorkspaceID, rsapiClientFactory);
				}).LifestyleTransient());

			container.Register(Component.For<ITokenProviderFactoryFactory>().ImplementedBy<TokenProviderFactoryFactory>()
				.LifestyleSingleton());
		}
	}
}