using System;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.DocumentFlow;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IdFileBuilding;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IntegrationPointRdoService;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobCancellation;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.SourceProvider;
using kCura.IntegrationPoints.Agent.Installer.Components;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter;
using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using kCura.IntegrationPoints.Agent.Sync;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core.AdlsHelpers;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Checkers;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.AutomatedWorkflows.SDK;
using ITaskFactory = kCura.IntegrationPoints.Agent.TaskFactory.ITaskFactory;

namespace kCura.IntegrationPoints.Agent.Installer
{
    internal class AgentInstaller : IWindsorInstaller
    {
        private readonly IAgentHelper _agentHelper;
        private readonly IScheduleRuleFactory _scheduleRuleFactory;

        public AgentInstaller(IAgentHelper agentHelper, IScheduleRuleFactory scheduleRuleFactory)
        {
            _agentHelper = agentHelper;
            _scheduleRuleFactory = scheduleRuleFactory;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            try
            {
                InstallContainer(container);
            }
            catch (Exception e)
            {
                IAPILog logger = _agentHelper.GetLoggerFactory().GetLogger().ForContext<AgentInstaller>();
                logger.LogError(e, "Unable to install container using AgentInstaller");
                throw;
            }
        }

        private void InstallContainer(IWindsorContainer container)
        {
            ConfigureContainer(container);

            container.Register(Component.For<IJobContextProvider>().Instance(new JobContextProvider()).LifestyleSingleton());

            ConfigureMonitoring(container);

            container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForAgent>().DynamicParameters((k, d) =>
            {
                IJobContextProvider jobContextProvider = k.Resolve<IJobContextProvider>();
                d.InsertTyped(jobContextProvider.Job.WorkspaceID);
            }).LifestyleTransient());
            container.Register(Component.For<IWorkspaceDBContext>().UsingFactoryMethod(k =>
            {
                IJobContextProvider jobContextProvider = k.Resolve<IJobContextProvider>();

                IDbContextFactory dbContextFactory = k.Resolve<IDbContextFactory>();
                return dbContextFactory.CreateWorkspaceDbContext(jobContextProvider.Job.WorkspaceID);
            }).LifestyleTransient().IsFallback());
            container.Register(Component.For<IEddsDBContext>().UsingFactoryMethod(k =>
            {
                IDbContextFactory dbContextFactory = k.Resolve<IDbContextFactory>();
                return dbContextFactory.CreatedEDDSDbContext();
            }).LifestyleTransient().IsFallback());

            container.Register(Component.For<Job>().UsingFactoryMethod(k =>
            {
                IJobContextProvider jobContextProvider = k.Resolve<IJobContextProvider>();
                return jobContextProvider.Job;
            }).LifestyleTransient().IsFallback());

            container.Register(Component.For<IRelativityObjectManagerService>().UsingFactoryMethod(k =>
            {
                IJobContextProvider jobContextProvider = k.Resolve<IJobContextProvider>();
                return new RelativityObjectManagerService(container.Resolve<IHelper>(), jobContextProvider.Job.WorkspaceID);
            }).LifestyleTransient().IsFallback());

            container.Register(Component.For<IDBContext>().UsingFactoryMethod(k =>
            {
                IJobContextProvider jobContextProvider = k.Resolve<IJobContextProvider>();
                return k.Resolve<IHelper>().GetDBContext(jobContextProvider.Job.WorkspaceID);
            }).LifestyleTransient());

            container.Register(Component.For<CurrentUser>().UsingFactoryMethod(k =>
            {
                IJobContextProvider jobContextProvider = k.Resolve<IJobContextProvider>();
                return new CurrentUser(userID: jobContextProvider.Job.SubmittedBy);
            }).LifestyleTransient());

            container.Register(Component.For<IScheduleRuleFactory>().UsingFactoryMethod(k => _scheduleRuleFactory, true).LifestyleTransient());
            container.Register(Component.For<IHelper>().UsingFactoryMethod(k => _agentHelper, true).LifestyleTransient());
            container.Register(Component.For<IAgentHelper>().UsingFactoryMethod(k => _agentHelper, true).LifestyleTransient());
            container.Register(Component.For<SyncWorker>().ImplementedBy<SyncWorker>().LifestyleTransient());
            container.Register(Component.For<SyncManager>().ImplementedBy<SyncManager>().LifestyleTransient());
            container.Register(Component.For<ExportServiceManager>().ImplementedBy<ExportServiceManager>().LifestyleTransient());
            container.Register(Component.For<ImportServiceManager>().ImplementedBy<ImportServiceManager>().LifestyleTransient());
            container.Register(Component.For<SyncEntityManagerWorker>().ImplementedBy<SyncEntityManagerWorker>().LifestyleTransient());
            container.Register(Component.For<ITaskExceptionService>().ImplementedBy<TaskExceptionService>().LifestyleTransient());
            container.Register(Component.For<ITaskExceptionMediator>().ImplementedBy<TaskExceptionMediator>().LifestyleTransient());
            container.Register(Component.For<SendEmailWorker>().ImplementedBy<SendEmailWorker>().LifestyleTransient());
            container.Register(Component.For<ExportManager>().ImplementedBy<ExportManager>().LifestyleTransient());
            container.Register(Component.For<ExportWorker>().ImplementedBy<ExportWorker>().LifestyleTransient());
            container.Register(Component.For<JobHistoryErrorServiceProvider>().ImplementedBy<JobHistoryErrorServiceProvider>().LifeStyle.BoundTo<ExportWorker>());
            container.Register(Component.For<IAgentValidator>().ImplementedBy<AgentValidator>().LifestyleTransient());
            container.Register(Component.For<IAutomatedWorkflowsManager>().ImplementedBy<AutomatedWorkflowsManager>());

            container.Register(Component.For<IJobSynchronizationChecker>().ImplementedBy<JobSynchronizationChecker>().LifestyleTransient());
            container.Register(Component.For<ITaskFactoryJobHistoryServiceFactory>().ImplementedBy<TaskFactoryJobHistoryServiceFactory>().LifestyleTransient());
            container.Register(Component.For<ITaskFactory>().ImplementedBy<TaskFactory.TaskFactory>().DependsOn(new { container }).LifestyleTransient());

            container.Register(Component
                .For<IAuthTokenGenerator>()
                .ImplementedBy<OAuth2TokenGenerator>()
                .LifestyleTransient());

            container.Register(Component.For<IDynamicProxyFactory>().ImplementedBy<DynamicProxyFactory>().LifestyleSingleton());
            container.Register(Component.For<IKeplerServiceFactory>().ImplementedBy<ServiceFactory>().LifestyleTransient());

            container.Register(
                Component
                    .For<IExportServiceObserversFactory>()
                    .ImplementedBy<ExportServiceObserversFactory>()
                    .LifestyleTransient());

            container.Register(Component
                .For<IExporterFactory>()
                .ImplementedBy<ExporterFactory>()
                .LifestyleTransient());

            container.Register(Component
                .For<IExternalServiceInstrumentationProvider>()
                .ImplementedBy<ExternalServiceInstrumentationProviderWithJobContext>()
                .LifestyleSingleton());

            container.AddEmailSender();

            ConfigureOtherProviderFlow(container);

            ConfigureScheduledSyncAppFlow(container);
        }

        private static void ConfigureContainer(IWindsorContainer container)
        {
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
            container.Kernel.AddFacility<TypedFactoryFacility>();

            container.Register(Component
                .For<ILazyComponentLoader>()
                .ImplementedBy<LazyOfTComponentLoader>());
        }

        private static void ConfigureMonitoring(IWindsorContainer container)
        {
            container.Register(Component.For<IMonitoringConfig>().ImplementedBy<MonitoringConfig>().LifestyleTransient());
            container.Register(Component.For<IDateTime>().ImplementedBy<DateTimeWrapper>().LifestyleTransient());
            container.Register(Component.For<ITimerFactory>().ImplementedBy<TimerFactory>().LifestyleTransient());
            container.Register(Component.For<IStopwatch>().ImplementedBy<StopwatchWrapper>().LifestyleTransient());

            container.Register(Component.For<IAppDomainMonitoringEnabler>().ImplementedBy<AppDomainMonitoringEnabler>().LifestyleTransient());
            container.Register(Component.For<IMemoryUsageReporter>().ImplementedBy<SystemAndApplicationUsageReporter>().LifestyleTransient());
            container.Register(Component.For<IProcessMemoryHelper>().ImplementedBy<ProcessMemoryHelper>().LifestyleTransient());
            container.Register(Component.For<ISystemHealthReporter>().ImplementedBy<SystemHealthReporter>().LifestyleTransient());
            container.Register(Component.For<IHealthStatisticReporter>().ImplementedBy<KeplerPingReporter>().LifestyleTransient());
            container.Register(Component.For<IHealthStatisticReporter>().ImplementedBy<DatabasePingReporter>().LifestyleTransient());
            container.Register(Component.For<IHealthStatisticReporter>().ImplementedBy<SystemStatisticsReporter>().LifestyleTransient());

            container.Register(Component.For<IHeartbeatReporter>().ImplementedBy<HeartbeatReporter>().LifestyleTransient());
        }

        private static void ConfigureOtherProviderFlow(IWindsorContainer container)
        {
            container.Register(Component.For<ICancellationTokenFactory>().ImplementedBy<CancellationTokenFactory>());
            container.Register(Component.For<IInstanceSettings>().ImplementedBy<InstanceSettings>());
            container.Register(Component.For<IJobDetailsService>().ImplementedBy<JobDetailsService>());
            container.Register(Component.For<IRelativityStorageService>().ImplementedBy<RelativityStorageService>().LifestyleSingleton());
            container.Register(Component.For<IAdlsHelper>().ImplementedBy<AdlsHelper>());
            container.Register(Component.For<IIdFilesBuilder>().ImplementedBy<IdFilesBuilder>().LifestyleTransient());
            container.Register(Component.For<ILoadFileBuilder>().ImplementedBy<LoadFileBuilder>().LifestyleTransient());
            container.Register(Component.For<ISourceProviderService>().ImplementedBy<SourceProviderService>().LifestyleTransient());
            container.Register(Component.For<IJobProgressHandler>().ImplementedBy<JobProgressHandler>().LifestyleTransient());
            container.Register(Component.For<IImportJobRunner>().ImplementedBy<ImportJobRunner>().LifestyleTransient());
            container.Register(Component.For<IJobHistoryService>().ImplementedBy<JobHistoryService>().LifestyleTransient());
            container.Register(Component.For<IIntegrationPointRdoService>().ImplementedBy<IntegrationPointRdoService>().LifestyleTransient());
            container.Register(Component.For<IJobHistoryErrorService>().ImplementedBy<JobHistoryErrorService>().LifestyleTransient());
            container.Register(Component.For<ICustomProviderTask>().ImplementedBy<CustomProviderTask>().LifestyleTransient());
            container.Register(Component.For<IImportApiRunnerFactory>().ImplementedBy<ImportApiRunnerFactory>().LifestyleTransient());
            container.Register(Component.For<IImportApiService>().ImplementedBy<ImportApiService>().LifestyleTransient());
            container.Register(Component.For<ICustomProviderFlowCheck>().ImplementedBy<CustomProviderFlowCheck>().LifestyleTransient());
            container.Register(Component.For<IDocumentImportSettingsBuilder>().ImplementedBy<DocumentImportSettingsBuilder>().LifestyleTransient());
            container.Register(Component.For<IRdoImportSettingsBuilder>().ImplementedBy<RdoImportSettingsBuilder>().LifestyleTransient());
            container.Register(Component.For<IItemLevelErrorHandler>().ImplementedBy<ItemLevelErrorHandler>().LifestyleTransient());
            container.Register(Component.For<DocumentImportApiRunner>().ImplementedBy<DocumentImportApiRunner>().LifestyleTransient());
            container.Register(Component.For<RdoImportApiRunner>().ImplementedBy<RdoImportApiRunner>().LifestyleTransient());
            container.Register(Component.For<IEntityFullNameService>().ImplementedBy<EntityFullNameService>().LifestyleTransient());
            container.Register(Component.For<IEntityFullNameObjectManagerService>().ImplementedBy<EntityFullNameObjectManagerService>().LifestyleTransient());
            container.Register(Component.For<INotificationService>().ImplementedBy<NotificationService>().LifestyleTransient());
        }

        private static void ConfigureScheduledSyncAppFlow(IWindsorContainer container)
        {
            container.Register(Component.For<IScheduledSyncTask>().ImplementedBy<ScheduledSyncTask>().LifestyleTransient());
        }
    }
}
