using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.kCura.IntegrationPoints.Agent;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Domain;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.CustodianManager;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.CustodianManager;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Email;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using CreateErrorRdo = kCura.ScheduleQueue.Core.Logging.CreateErrorRdo;
using IExporterFactory = kCura.IntegrationPoints.Core.Factories.IExporterFactory;
using ITaskFactory = kCura.IntegrationPoints.Agent.Tasks.ITaskFactory;

namespace kCura.IntegrationPoints.Agent.Installer
{
	internal class AgentInstaller : IWindsorInstaller
	{
		private readonly IAgentHelper _agentHelper;
		private readonly Job _job;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;

		public AgentInstaller(IAgentHelper agentHelper, Job job, IScheduleRuleFactory scheduleRuleFactory)
		{
			_agentHelper = agentHelper;
			_job = job;
			_scheduleRuleFactory = scheduleRuleFactory;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			try
			{
				InstallContainer(container, store);
			}
			catch (Exception e)
			{
				IAPILog logger = _agentHelper.GetLoggerFactory().GetLogger().ForContext<AgentInstaller>();
				logger.LogError(e, "Unable to install container using AgentInstaller");
				throw;
			}
		}

		private void InstallContainer(IWindsorContainer container, IConfigurationStore store)
		{
			string currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			bool isIlMergedAssembly = currentAssemblyName == "kCura.IntegrationPoints";

			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
			container.Kernel.AddFacility<TypedFactoryFacility>();

			const string CORE_ASSEMBLY_NAME = "kCura.IntegrationPoints.Core";

			container.Register(Component.For<IScheduleRuleFactory>().UsingFactoryMethod(k => _scheduleRuleFactory, true));
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => _agentHelper, true));
			container.Register(Component.For<IAgentHelper>().UsingFactoryMethod(k => _agentHelper, true));
			container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForAgent>()
				.DependsOn(Dependency.OnValue<int>(_job.WorkspaceID)));
			container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>());
			container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>());
			container.Register(Component.For<IWorkspaceDBContext>().UsingFactoryMethod(k => new WorkspaceContext(_agentHelper.GetDBContext(_job.WorkspaceID))));
			container.Register(Component.For<ICredentialProvider>().ImplementedBy<TokenCredentialProvider>());
			container.Register(Component.For<Job>().UsingFactoryMethod(k => _job));
			container.Register(
				Component.For<GetApplicationBinaries>()
					.ImplementedBy<GetApplicationBinaries>()
					.DependsOn(Property.ForKey("eddsDBcontext").Eq(_agentHelper.GetDBContext(-1)))
					.LifestyleTransient());

			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod(k =>
					k.Resolve<RsapiClientFactory>().CreateClientForWorkspace(_job.WorkspaceID, ExecutionIdentity.System)));
			container.Register(Component.For<IRSAPIService>().Instance(new RSAPIService(container.Resolve<IHelper>(), _job.WorkspaceID)).LifestyleTransient());

			if (container.Kernel.HasComponent(typeof(IRelativityConfigurationFactory)) == false)
			{
				container.Register(
					Component.For<IRelativityConfigurationFactory>()
						.ImplementedBy<RelativityConfigurationFactory>()
						.LifestyleSingleton());
			}

			container.Register(Component.For<ISendable>()
				.ImplementedBy<SMTP>()
				.DependsOn(
					Dependency.OnValue<EmailConfiguration>(container.Resolve<IRelativityConfigurationFactory>().GetConfiguration())));

			container.Register(Component.For<IOnBehalfOfUserClaimsPrincipalFactory>()
				.ImplementedBy<OnBehalfOfUserClaimsPrincipalFactory>()
				.LifestyleTransient());

			container.Register(Component.For<SyncWorker>().ImplementedBy<SyncWorker>().LifeStyle.Transient);
			container.Register(Component.For<SyncManager>().ImplementedBy<SyncManager>().LifeStyle.Transient);
			container.Register(Component.For<ExportServiceManager>().ImplementedBy<ExportServiceManager>().LifeStyle.Transient);
			container.Register(Component.For<SyncCustodianManagerWorker>().ImplementedBy<SyncCustodianManagerWorker>().LifeStyle.Transient);
			container.Register(Component.For<CreateErrorRdo>().ImplementedBy<CreateErrorRdo>().LifeStyle.Transient);
			container.Register(Component.For<ITaskFactory>().AsFactory(x => x.SelectedWith(new TaskComponentSelector())).LifeStyle.Transient);
			if (container.Kernel.HasComponent(typeof(ISerializer)) == false)
			{
				container.Register(Component.For<ISerializer>().ImplementedBy<JSONSerializer>().LifestyleTransient());
			}
			container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(f => f.Resolve<IHelper>().GetServicesManager()));

			container.Register(Component.For<SendEmailManager>().ImplementedBy<SendEmailManager>().LifeStyle.Transient);
			container.Register(Component.For<SendEmailWorker>().ImplementedBy<SendEmailWorker>().LifeStyle.Transient);
			container.Register(Component.For<JobStatisticsService>().ImplementedBy<JobStatisticsService>().LifeStyle.Transient);

			container.Register(Component.For<ExportManager>().ImplementedBy<ExportManager>().LifeStyle.Transient);
			container.Register(Component.For<ExportWorker>().ImplementedBy<ExportWorker>()
				.DependsOn(Dependency.OnComponent<ISynchronizerFactory, ExportDestinationSynchronizerFactory>())
				.LifeStyle.Transient);
			container.Register(
				Component.For<JobHistoryErrorServiceProvider>()
					.ImplementedBy<JobHistoryErrorServiceProvider>()
					.LifeStyle.BoundTo<ExportWorker>());

			container.Register(Component.For<RsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifestyleTransient());
			container.Register(Component.For<IJobManager>().ImplementedBy<AgentJobManager>().LifestyleTransient());
			container.Register(Component.For<IJobService>().ImplementedBy<JobService>().LifestyleTransient());

			if (container.Kernel.HasComponent(typeof(IIntegrationPointService)) == false)
			{
				container.Register(Component.For<IIntegrationPointService>().ImplementedBy<IntegrationPointService>().LifestyleTransient());
			}

			container.Register(Component.For<IGuidService>().ImplementedBy<DefaultGuidService>().LifestyleTransient());
			container.Register(Component.For<JobHistoryErrorService>().ImplementedBy<JobHistoryErrorService>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryErrorService>()
				.UsingFactoryMethod(k => k.Resolve<JobHistoryErrorService>()).LifestyleTransient());
			container.Register(Component.For<IManagerFactory>().ImplementedBy<ManagerFactory>().LifestyleTransient());
			container.Register(Component.For<IContextContainerFactory>().ImplementedBy<ContextContainerFactory>().LifestyleSingleton());
			container.Register(Component.For<IBatchStatus>().ImplementedBy<BatchEmail>().LifeStyle.Transient);
			container.Register(Component.For<IBatchStatus>().ImplementedBy<JobHistoryBatchUpdateStatus>().LifeStyle.Transient);
			container.Register(Component.For<IDataProviderFactory>().ImplementedBy<AppDomainFactory>().LifestyleTransient());
			var guid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);
			container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().DependsOn(Dependency.OnValue<Guid>(guid)).LifestyleTransient());
			container.Register(Component.For<JobTracker>().ImplementedBy<JobTracker>().LifeStyle.Transient);
			container.Register(Component.For<IJobHistoryService>().ImplementedBy<JobHistoryService>().LifestyleTransient());
			container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<GeneralWithCustodianRdoSynchronizerFactory>().DependsOn(new {container}).LifestyleTransient());
			container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<ExportDestinationSynchronizerFactory>().DependsOn(new {container}).LifestyleTransient());
			container.Register(Component.For<DomainHelper>().ImplementedBy<DomainHelper>().LifestyleTransient());
			container.Register(Component.For<ISourcePluginProvider>().ImplementedBy<DefaultSourcePluginProvider>().LifestyleTransient());
			container.Register(Component.For<RelativityFeaturePathService>().ImplementedBy<RelativityFeaturePathService>().LifeStyle.Transient);
			container.Register(Component.For<TaskParameterHelper>().ImplementedBy<TaskParameterHelper>().LifestyleTransient());
			container.Register(Component.For<IExporterFactory>().ImplementedBy<ExporterFactory>().LifestyleTransient());
			container.Register(Component.For<IJobStatusUpdater>().ImplementedBy<JobStatusUpdater>().LifeStyle.Transient);
			container.Register(Component.For<IManagerQueueService>().ImplementedBy<ManagerQueueService>().LifestyleTransient());
			container.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>().LifestyleSingleton());
			// TODO: This is kind of cruddy, see if we can only use this repository through the RepositoryFactory -- biedrzycki: April 6th, 2016
			container.Register(
				Component.For<IWorkspaceRepository>()
					.ImplementedBy<KeplerWorkspaceRepository>()
					.UsingFactoryMethod(k => k.Resolve<IRepositoryFactory>().GetWorkspaceRepository())
					.LifestyleTransient());
			container.Register(Component.For<ISourceWorkspaceManager>().ImplementedBy<SourceWorkspaceManager>().LifestyleTransient());
			container.Register(Component.For<ISourceJobManager>().ImplementedBy<SourceJobManager>().LifestyleTransient());
			container.Register(Component.For<ExportProcessRunner>().ImplementedBy<ExportProcessRunner>());
			container.Register(Component.For<IExportProcessBuilder>().ImplementedBy<ExportProcessBuilder>());
			container.Register(Component.For<IExportSettingsBuilder>().ImplementedBy<ExportSettingsBuilder>());
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizerPush>().Named(typeof(RdoSynchronizerPush).AssemblyQualifiedName).LifeStyle.Transient);
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizerPull>().Named(typeof(RdoSynchronizerPull).AssemblyQualifiedName).LifeStyle.Transient);
			container.Register(
				Component.For<IDataSynchronizer>().ImplementedBy<RdoCustodianSynchronizer>().Named(typeof(RdoCustodianSynchronizer).AssemblyQualifiedName).LifeStyle.Transient);
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<ExportSynchroznizer>().Named(typeof(ExportSynchroznizer).AssemblyQualifiedName).LifeStyle.Transient);
			container.Register(Component.For<IImportApiFactory>().ImplementedBy<ImportApiFactory>().LifeStyle.Transient);
			container.Register(Component.For<ISystemEventLoggingService>().ImplementedBy<SystemEventLoggingService>().LifeStyle.Transient);
			container.Register(Component.For<IRelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());
			container.Register(Component.For<IInstanceSettingRepository>().ImplementedBy<InstanceSettingRepository>().LifestyleTransient());

			#region Data

			const string DATA_ASSEMBLY_NAME = "kCura.IntegrationPoints.Data";

			#region Convention

			var excludedQueryClassNames = new HashSet<string>(new[]
			{
				typeof(CreateCustodianManagerResourceTable).Name,
				typeof(GetApplicationGuid).Name,
				typeof(GetJobCustodianManagerLinks).Name,
				typeof(GetJobsCount).Name
			});
			FromAssemblyDescriptor fromAssemblyDescriptor = null;
			if (isIlMergedAssembly)
			{
				fromAssemblyDescriptor = Classes.FromThisAssembly();
			}
			else
			{
				fromAssemblyDescriptor = Classes.FromAssemblyNamed(DATA_ASSEMBLY_NAME);
			}
			container.Register(
				fromAssemblyDescriptor
					.InNamespace("kCura.IntegrationPoints.Data.Queries")
					.If(x => !x.GetInterfaces().Any())
					.If(x => !excludedQueryClassNames.Contains(x.Name))
					.Configure(c => c.LifestyleTransient()));

			#endregion

			container.Register(Component.For<RSAPIRdoQuery>().ImplementedBy<RSAPIRdoQuery>().LifeStyle.Transient);
			container.Register(Component.For<IChoiceQuery>().ImplementedBy<ChoiceQuery>().LifeStyle.Transient);
			container.Register(Component.For<IFileQuery>().ImplementedBy<FileQuery>().LifeStyle.Transient);

			#endregion

			#region Keyword

			#region Convention

			if (!isIlMergedAssembly)
			{
				fromAssemblyDescriptor = Classes.FromAssemblyNamed(CORE_ASSEMBLY_NAME);
			}

			container.Register(
				fromAssemblyDescriptor
					.BasedOn<IKeyword>()
					.WithService.DefaultInterfaces()
					.Configure(x => x.LifestyleTransient()));

			#endregion

			container.Register(Component.For<KeywordConverter>().ImplementedBy<KeywordConverter>().LifeStyle.Transient);
			container.Register(Component.For<KeywordFactory>().ImplementedBy<KeywordFactory>().LifeStyle.Transient);

			#endregion

			#region FilesDestinationProvider

			const string FILESDESTINATIONPROVIDER_ASSEMBLY_NAME = "kCura.IntegrationPoints.FilesDestinationProvider.Core";

			#region Convention

			// The following classes are ignored as they are either registered later in a specific fashion
			// or are not intended to be injected.
			var excludedFdpClassNames = new HashSet<string>(new[]
			{
				typeof(ExportUserNotification).Name,
				typeof(ExportLoggingMediator).Name,
				typeof(ExportFieldsService).Name,
				typeof(ExportSettingsValidationService).Name,
				typeof(ArtifactTreeService).Name,
				typeof(ProductionService).Name,
				typeof(CaseManagerFactory).Name,
				typeof(StoppableExporterFactory).Name,
				typeof(CoreAuditManager).Name,
				typeof(CoreFieldManager).Name,
				typeof(CoreExportManager).Name,
				typeof(CoreProductionManager).Name,
				typeof(CoreSearchManager).Name,
				typeof(CoreServiceFactory).Name,
				typeof(CompositeLoggingMediator).Name,
				typeof(SearchManagerFactory).Name,
				typeof(ViewService).Name,
				typeof(SavedSearchesTreeService).Name
			});

			var excludedFdpInterfaceNames = new HashSet<string>(new[]
			{
				typeof(IExporter).Name
			});

			if (!isIlMergedAssembly)
			{
				fromAssemblyDescriptor = Classes.FromAssemblyNamed(FILESDESTINATIONPROVIDER_ASSEMBLY_NAME);
			}

			container.Register(
				fromAssemblyDescriptor
					.IncludeNonPublicTypes()
					.InNamespace(FILESDESTINATIONPROVIDER_ASSEMBLY_NAME, true)
					.If(x => x.GetInterfaces().Any())
					.If(x => !x.GetInterfaces().Select(y => y.Name).Intersect(excludedFdpInterfaceNames).Any())
					.If(x => !excludedFdpClassNames.Contains(x.Name))
					.WithService.DefaultInterfaces()
					.Configure(x => x.LifestyleTransient()));

			#endregion

			container.Register(Component.For<IManagerFactory<ISearchManager>>()
				.ImplementedBy<SearchManagerFactory>());
			container.Register(Component.For<LoggingMediatorFactory>().ImplementedBy<LoggingMediatorFactory>());
			container.Register(Component.For<ICompositeLoggingMediator>().UsingFactory((LoggingMediatorFactory f) => f.Create()));
			container.Register(Component.For<IUserMessageNotification, IUserNotification>().ImplementedBy<ExportUserNotification>());
			container.Register(Component.For<ICaseManagerFactory>().ImplementedBy<CaseManagerFactory>());
			container.Register(Component.For<FilesDestinationProvider.Core.SharedLibrary.IExporterFactory>()
				.ImplementedBy<StoppableExporterFactory>());

			#endregion
		}
	}
}