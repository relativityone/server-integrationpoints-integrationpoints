using System;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Installer.Components;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter;
using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using kCura.IntegrationPoints.Agent.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Interfaces;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.RelativitySync;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
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

            container.Register(Component.For<IRelativitySyncConstrainsChecker>().ImplementedBy<RelativitySyncConstrainsChecker>());
            container.Register(Component.For<IRelativitySyncAppIntegration>().ImplementedBy<RelativitySyncAppIntegration>());

            container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForAgent>().DynamicParameters((k, d) =>
            {
                IJobContextProvider jobContextProvider = k.Resolve<IJobContextProvider>();
                d.InsertTyped(jobContextProvider.Job.WorkspaceID);
            }).LifestyleTransient());

            container.Register(Component.For<IWorkspaceDBContext>().UsingFactoryMethod(k =>
            {
                IJobContextProvider jobContextProvider = k.Resolve<IJobContextProvider>();
                return new WorkspaceDBContext(_agentHelper.GetDBContext(jobContextProvider.Job.WorkspaceID));
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

            container.Register(
                Component
                    .For<IExportServiceObserversFactory>()
                    .ImplementedBy<ExportServiceObserversFactory>()
                    .LifestyleTransient()
                );

            container.Register(Component
                .For<Core.Factories.IExporterFactory>()
                .ImplementedBy<ExporterFactory>()
                .LifestyleTransient()
            );

            container.Register(Component
                .For<IExternalServiceInstrumentationProvider>()
                .ImplementedBy<ExternalServiceInstrumentationProviderWithJobContext>()
                .LifestyleSingleton()
            );
            container.Register(Component
                .For<IInstanceSettingsBundle>()
                .UsingFactoryMethod(kernel => kernel.Resolve<IHelper>().GetInstanceSettingBundle())
                .LifestyleTransient()
            );

            container.AddEmailSender();
        }

        private static void ConfigureContainer(IWindsorContainer container)
        {
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
            container.Kernel.AddFacility<TypedFactoryFacility>();

            container.Register(Component
                .For<ILazyComponentLoader>()
                .ImplementedBy<LazyOfTComponentLoader>()
            );
        }

        private static void ConfigureMonitoring(IWindsorContainer container)
        {
            container.Register(Component.For<IMonitoringConfig>().ImplementedBy<MonitoringConfig>().LifestyleTransient());
            container.Register(Component.For<IDateTime>().ImplementedBy<DateTimeWrapper>().LifestyleTransient());
            container.Register(Component.For<ITimerFactory>().ImplementedBy<TimerFactory>().LifestyleTransient());

            container.Register(Component.For<IAppDomainMonitoringEnabler>().ImplementedBy<AppDomainMonitoringEnabler>().LifestyleTransient());
            container.Register(Component.For<IMemoryUsageReporter>().ImplementedBy<SystemAndApplicationUsageReporter>().LifestyleTransient());
            container.Register(Component.For<IProcessMemoryHelper>().ImplementedBy<ProcessMemoryHelper>().LifestyleTransient());
            container.Register(Component.For<ISystemHealthReporter>().ImplementedBy<SystemHealthReporter>().LifestyleTransient());
            container.Register(Component.For<IHealthStatisticReporter>().ImplementedBy<FileShareDiskUsageReporter>().LifestyleTransient());
            container.Register(Component.For<IHealthStatisticReporter>().ImplementedBy<KeplerPingReporter>().LifestyleTransient());
            container.Register(Component.For<IHealthStatisticReporter>().ImplementedBy<DatabasePingReporter>().LifestyleTransient());
            container.Register(Component.For<IHealthStatisticReporter>().ImplementedBy<SystemStatisticsReporter>().LifestyleTransient());

            container.Register(Component.For<IHeartbeatReporter>().ImplementedBy<HeartbeatReporter>().LifestyleTransient());

        }
    }
}
