using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.DbContext;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Progress;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Utils;
using Relativity.Sync.Utils.Workarounds;
using Relativity.Telemetry.APM;
using Relativity.Toggles;

namespace Relativity.Sync
{
    internal sealed class ContainerFactory : IContainerFactory
    {
        public void RegisterSyncDependencies(ContainerBuilder containerBuilder, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration configuration, IAPILog logger)
        {
            const string syncJob = nameof(SyncJob);
            containerBuilder.RegisterType<SyncJob>().Named(syncJob, typeof(ISyncJob));
            containerBuilder.RegisterDecorator<ISyncJob>((context, job) => new SyncJobWithUnhandledExceptionLogging(job, context.Resolve<IAppDomain>(), context.Resolve<IAPILog>()), syncJob);

            containerBuilder.RegisterInstance(logger).As<IAPILog>();
            containerBuilder.RegisterInstance(syncJobParameters).As<SyncJobParameters>();
            containerBuilder.RegisterInstance(configuration).As<SyncJobExecutionConfiguration>();
            containerBuilder.RegisterInstance(relativityServices).As<IRelativityServices>();
            containerBuilder.Register(_ => relativityServices.Helper.GetServicesManager()).As<IServicesMgr>();
            containerBuilder.RegisterInstance(relativityServices.APM).As<IAPM>();
            containerBuilder.RegisterInstance(relativityServices.Helper).As<IHelper>();
            containerBuilder.RegisterType<WorkspaceGuidService>().As<IWorkspaceGuidService>().SingleInstance();
            containerBuilder.RegisterType<SyncExecutionContextFactory>().As<ISyncExecutionContextFactory>();
            containerBuilder.RegisterType<AppDomainWrapper>().As<IAppDomain>();
            containerBuilder.RegisterType<MemoryCacheWrapper>().As<IMemoryCache>();
            containerBuilder.RegisterType<DateTimeWrapper>().As<IDateTime>();
            containerBuilder.RegisterType<WrapperForRandom>().As<IRandom>();
            containerBuilder.RegisterType<JSONSerializer>().As<ISerializer>();
            containerBuilder.RegisterType<ProgressStateCounter>().As<IProgressStateCounter>();
            containerBuilder.RegisterType<SyncJobProgress>().As<IProgress<SyncJobState>>();
            containerBuilder.RegisterType<JobEndMetricsServiceFactory>().As<IJobEndMetricsServiceFactory>();
            containerBuilder.RegisterType<RipWorkarounds>().As<IRipWorkarounds>();
            containerBuilder.RegisterType<JobProgressUpdater>().As<IJobProgressUpdater>();
            containerBuilder.RegisterType<TimerFactory>().As<ITimerFactory>();

            containerBuilder.RegisterInstance(ToggleProvider.Current).As<IToggleProvider>().SingleInstance().PreserveExistingDefaults();

            containerBuilder.Register(c =>
            {
                IDBContext dbContext = relativityServices.GetEddsDbContext();
                return new EddsDbContext(dbContext);
            }).As<IEddsDbContext>().InstancePerLifetimeScope();

            const string command = "command";
            containerBuilder.RegisterGeneric(typeof(Command<>)).Named(command, typeof(ICommand<>));
            containerBuilder.RegisterGenericDecorator(typeof(CommandWithMetrics<>), typeof(ICommand<>), command);

            IEnumerable<IInstaller> installers = GetInstallersInCurrentAssembly();
            foreach (IInstaller installer in installers)
            {
                installer.Install(containerBuilder);
            }

            containerBuilder.RegisterType<PipelineSelectorConfiguration>().As<IPipelineSelectorConfiguration>();
            containerBuilder.RegisterType<PipelineSelector>().AsImplementedInterfaces().SingleInstance();
            containerBuilder.RegisterType<ImportServiceConfiguration>().As<IImportServiceConfiguration>();

            containerBuilder.RegisterType<RdoGuidProvider>().AsImplementedInterfaces();
            containerBuilder.RegisterType<RdoManager>().AsImplementedInterfaces();

            IPipelineBuilder pipelineBuilder = new PipelineBuilder();
            pipelineBuilder.RegisterFlow(containerBuilder);

            Type[] validatorTypes = GetValidatorTypesExcept<ValidatorWithMetrics>();
            foreach (Type validatorType in validatorTypes)
            {
                string decoratorName = validatorType.FullName;
                containerBuilder.RegisterType(validatorType).Named(decoratorName, typeof(IValidator));
                containerBuilder.RegisterDecorator<IValidator>(
                    (context, validator) =>
                    new ValidatorWithMetrics(validator, context.Resolve<ISyncMetrics>(), context.Resolve<IStopwatch>()), decoratorName);
            }
        }

        private static Type[] GetValidatorTypesExcept<T>()
        {
            return
                typeof(IValidator).Assembly
                    .GetTypes()
                    .Where(t => !t.IsAbstract &&
                                t.IsAssignableTo<IValidator>() &&
                                !t.IsAssignableTo<T>())
                    .ToArray();
        }

        private static IEnumerable<IInstaller> GetInstallersInCurrentAssembly()
        {
            return Assembly
                .GetCallingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsAssignableTo<IInstaller>())
                .Select(t => (IInstaller)Activator.CreateInstance(t));
        }
    }
}
