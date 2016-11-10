using System;
using SystemInterface.IO;
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
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			var guid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

			container.Register(Component.For<ISerializer>().ImplementedBy<JSONSerializer>().LifestyleSingleton());
			container.Register(Component.For<IObjectTypeRepository>().ImplementedBy<RsapiObjectTypeRepository>().UsingFactoryMethod(x =>
			{
				IServiceContextHelper contextHelper = x.Resolve<IServiceContextHelper>();
				IHelper helper = x.Resolve<IHelper>();
				return new RsapiObjectTypeRepository(helper, contextHelper.WorkspaceID);
			}).LifestyleTransient());

			container.Register(Component.For<IContextContainerFactory>().ImplementedBy<ContextContainerFactory>().LifestyleTransient());
			container.Register(Component.For<IErrorService>().ImplementedBy<ErrorService>().Named("ErrorService").LifestyleTransient());
			container.Register(Component.For<ObjectTypeService>().ImplementedBy<ObjectTypeService>().LifestyleTransient());
			container.Register(Component.For<ISourcePluginProvider>().ImplementedBy<DefaultSourcePluginProvider>().LifestyleTransient());
			container.Register(Component.For<IDataProviderFactory>().ImplementedBy<AppDomainFactory>().LifestyleTransient());
			container.Register(Component.For<DomainHelper>().ImplementedBy<DomainHelper>().LifestyleSingleton());
			container.Register(Component.For<IJobManager>().ImplementedBy<AgentJobManager>().LifestyleTransient());
			container.Register(Component.For<IJobService>().ImplementedBy<JobService>().LifestyleTransient());
			container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestyleTransient());
			container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());
			container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().DependsOn(Dependency.OnValue<Guid>(guid)).LifestyleTransient());
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizerPush>().Named(typeof(RdoSynchronizerPush).AssemblyQualifiedName).LifestyleTransient());
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizerPull>().Named(typeof(RdoSynchronizerPull).AssemblyQualifiedName).LifestyleTransient());
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoCustodianSynchronizer>().Named(typeof(RdoCustodianSynchronizer).AssemblyQualifiedName).LifestyleTransient());
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<ExportSynchroznizer>().Named(typeof(ExportSynchroznizer).AssemblyQualifiedName).LifestyleTransient());
			container.Register(Component.For<IRdoSynchronizerProvider>().ImplementedBy<RdoSynchronizerProvider>().LifestyleTransient());
			container.Register(Component.For<IRelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointService>().ImplementedBy<IntegrationPointService>().LifestyleTransient());
			container.Register(Component.For<GetSourceProviderRdoByIdentifier>().ImplementedBy<GetSourceProviderRdoByIdentifier>().LifestyleTransient());
			container.Register(Component.For<IBatchStatus>().ImplementedBy<BatchEmail>().LifestyleTransient());
			container.Register(Component.For<IBatchStatus>().ImplementedBy<JobHistoryBatchUpdateStatus>().LifestyleTransient());
			container.Register(Component.For<ISourceTypeFactory>().ImplementedBy<SourceTypeFactory>().LifestyleTransient());
			container.Register(Component.For<IDestinationTypeFactory>().ImplementedBy<DestinationTypeFactory>().LifestyleTransient());
			container.Register(Component.For<RsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifestyleTransient());
			container.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceRepository>().ImplementedBy<KeplerWorkspaceRepository>().UsingFactoryMethod((k) => k.Resolve<IRepositoryFactory>().GetWorkspaceRepository()).LifestyleTransient());
			container.Register(Component.For<RdoFilter>().ImplementedBy<RdoFilter>().LifestyleTransient());
			container.Register(Component.For<UserService>().ImplementedBy<UserService>().LifestyleTransient());
			container.Register(Component.For<ChoiceService>().ImplementedBy<ChoiceService>().LifestyleTransient());
			container.Register(Component.For<CustodianService>().ImplementedBy<CustodianService>().LifestyleTransient());
			container.Register(Component.For<ITabService>().ImplementedBy<RSAPITabService>().LifestyleTransient());
			container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<GeneralWithCustodianRdoSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());
			container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<ExportDestinationSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());
			container.Register(Component.For<IProviderFactory>().ImplementedBy<DefaultProviderFactory>().DependsOn(new { windsorContainer = container }).LifestyleTransient());
			container.Register(Component.For<IManagerQueueService>().ImplementedBy<ManagerQueueService>().LifestyleTransient());
			container.Register(Component.For<IGuidService>().ImplementedBy<DefaultGuidService>().LifestyleSingleton());
			container.Register(Component.For<IJobHistoryService>().ImplementedBy<JobHistoryService>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryErrorService>().ImplementedBy<JobHistoryErrorService>().LifestyleTransient());
			container.Register(Component.For<IJobStatusUpdater>().ImplementedBy<JobStatusUpdater>().LifestyleTransient());
			container.Register(Component.For<JobTracker>().ImplementedBy<JobTracker>().LifestyleTransient());
			container.Register(Component.For<TaskParameterHelper>().ImplementedBy<TaskParameterHelper>().LifestyleTransient());
			container.Register(Component.For<IImportApiFactory>().ImplementedBy<ImportApiFactory>().LifestyleTransient());
			container.Register(Component.For<RelativityFeaturePathService>().ImplementedBy<RelativityFeaturePathService>().LifestyleTransient());
			container.Register(Component.For<IExporterFactory>().ImplementedBy<ExporterFactory>().LifestyleTransient());
			container.Register(Component.For<IManagerFactory>().ImplementedBy<ManagerFactory>().LifestyleTransient());
			container.Register(Component.For<IEncryptionManager>().ImplementedBy<DefaultEncryptionManager>().LifestyleSingleton());
			container.Register(Component.For<ISourceWorkspaceManager>().ImplementedBy<SourceWorkspaceManager>().LifestyleTransient());
			container.Register(Component.For<ISourceJobManager>().ImplementedBy<SourceJobManager>().LifestyleTransient());
			container.Register(Component.For<IOnBehalfOfUserClaimsPrincipalFactory>().ImplementedBy<OnBehalfOfUserClaimsPrincipalFactory>().LifestyleTransient());
			container.Register(Component.For<ISavedSearchesTreeService>().ImplementedBy<SavedSearchesTreeService>().LifestyleTransient());
			container.Register(Component.For<IDirectoryTreeCreator<JsTreeItemDTO>>().ImplementedBy<DirectoryTreeCreator<JsTreeItemDTO>>().LifestyleTransient());
			container.Register(Component.For<IArtifactTreeCreator>().ImplementedBy<ArtifactTreeCreator>().LifestyleTransient());
			container.Register(Component.For<ISavedSearchesTreeCreator>().ImplementedBy<SavedSearchesTreeCreator>());
			container.Register(Component.For<IResourcePoolManager>().ImplementedBy<ResourcePoolManager>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceManager>().ImplementedBy<WorkspaceManager>().LifestyleTransient());
			container.Register(Component.For<IDirectory>().ImplementedBy<LongPathDirectory>().LifestyleTransient());
			container.Register(Component.For<JobStatisticsService>().ImplementedBy<JobStatisticsService>().LifestyleTransient());
			container.Register(Component.For<KeywordConverter>().ImplementedBy<KeywordConverter>().LifestyleTransient());
			container.Register(Component.For<KeywordFactory>().ImplementedBy<KeywordFactory>().LifestyleTransient());
		}
	}
}