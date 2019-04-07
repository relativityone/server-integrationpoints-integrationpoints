using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;using kCura.Apps.Common.Utils.Serializers;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync
{
	internal sealed class ContainerFactory : IContainerFactory
	{
		public void RegisterSyncDependencies(ContainerBuilder containerBuilder, SyncJobParameters syncJobParameters, SyncJobExecutionConfiguration configuration, ISyncLog logger)
		{
			CorrelationId correlationId = new CorrelationId(syncJobParameters.CorrelationId);

			const string syncJob = nameof(SyncJob);
			containerBuilder.RegisterType<SyncJob>().Named(syncJob, typeof(ISyncJob));
			containerBuilder.RegisterDecorator<ISyncJob>((context, job) => new SyncJobWithUnhandledExceptionLogging(job, context.Resolve<IAppDomain>(), context.Resolve<ISyncLog>()), syncJob);

			containerBuilder.RegisterInstance(new ContextLogger(correlationId, logger)).As<ISyncLog>();
			containerBuilder.RegisterInstance(syncJobParameters).As<SyncJobParameters>();
			containerBuilder.RegisterInstance(correlationId).As<CorrelationId>();
			containerBuilder.RegisterInstance(configuration).As<SyncJobExecutionConfiguration>();
			containerBuilder.RegisterType<SyncExecutionContextFactory>().As<ISyncExecutionContextFactory>();
			containerBuilder.RegisterType<AppDomainWrapper>().As<IAppDomain>();
			containerBuilder.RegisterType<JSONSerializer>().As<ISerializer>();
			containerBuilder.RegisterType<ProgressStateCounter>().As<IProgressStateCounter>();
			containerBuilder.RegisterType<SyncJobProgress>().As<IProgress<SyncJobState>>();
			
			containerBuilder.RegisterType<SourceWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>>();
			containerBuilder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();
			containerBuilder.RegisterType<DestinationWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration>>();
			containerBuilder.RegisterType<DestinationWorkspaceTagsCreationExecutor>().As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();
			
			IPipelineBuilder pipelineBuilder = new PipelineBuilder();
			pipelineBuilder.RegisterFlow(containerBuilder);

			const string command = "command";
			containerBuilder.RegisterGeneric(typeof(Command<>)).Named(command, typeof(ICommand<>));
			containerBuilder.RegisterGenericDecorator(typeof(CommandWithMetrics<>), typeof(ICommand<>), command);

			IEnumerable<IInstaller> installers = GetInstallersInCurrentAssembly();
			foreach (IInstaller installer in installers)
			{
				installer.Install(containerBuilder);
			}

			Type[] validatorTypes = GetValidatorTypesExcept<ValidatorWithMetrics>();
			foreach (Type validatorType in validatorTypes)
			{
				string decoratorName = validatorType.FullName;
				containerBuilder.RegisterType(validatorType).Named(decoratorName, typeof(IValidator));
				containerBuilder.RegisterDecorator<IValidator>((context, validator) => 
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
				.Select(t => (IInstaller) Activator.CreateInstance(t));
		}
	}
}