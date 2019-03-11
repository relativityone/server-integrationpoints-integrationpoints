using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Autofac;
using Banzai.Logging;
using Relativity.API;
using Relativity.Sync.Authentication;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync
{
	/// <inheritdoc />
	public sealed class SyncJobFactory : ISyncJobFactory
	{
		private readonly IPipelineBuilder _pipelineBuilder;

		/// <inheritdoc />
		public SyncJobFactory()
		{
			_pipelineBuilder = new PipelineBuilder();
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters)
		{
			return Create(container, GetInstallersInExecutingAssembly(), syncJobParameters, new SyncConfiguration(), new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, ISyncLog logger)
		{
			return Create(container, GetInstallersInExecutingAssembly(), syncJobParameters, new SyncConfiguration(), logger);
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncConfiguration configuration)
		{
			return Create(container, GetInstallersInExecutingAssembly(), syncJobParameters, configuration, new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, IEnumerable<IInstaller> installers, SyncJobParameters syncJobParameters)
		{
			return Create(container, installers, syncJobParameters, new SyncConfiguration(), new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, IEnumerable<IInstaller> installers, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			if (installers == null)
			{
				throw new ArgumentNullException(nameof(installers));
			}

			if (syncJobParameters == null)
			{
				throw new ArgumentNullException(nameof(syncJobParameters));
			}

			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			try
			{
				LogWriter.SetFactory(new SyncLogWriterFactory(logger));

				using (ILifetimeScope scope = container.BeginLifetimeScope(builder => RegisterDependencies(builder, installers, syncJobParameters, configuration, logger)))
				{
					return scope.Resolve<ISyncJob>();
				}
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to create Sync job {correlationId}.", syncJobParameters.CorrelationId);
				throw new SyncException("Unable to create Sync job. See inner exception for more details.", e, syncJobParameters.CorrelationId);
			}
		}

		private IEnumerable<IInstaller> GetInstallersInExecutingAssembly()
		{
			return Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<IInstaller>())
				.Select(t => (IInstaller)Activator.CreateInstance(t));
		}

		private void RegisterDependencies(ContainerBuilder builder, IEnumerable<IInstaller> installers, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger)
		{
			CorrelationId correlationId = new CorrelationId(syncJobParameters.CorrelationId);

			const string syncJob = nameof(SyncJob);
			builder.RegisterType<SyncJob>().Named(syncJob, typeof(ISyncJob));
			builder.RegisterDecorator<ISyncJob>((context, job) => new SyncJobWithUnhandledExceptionLogging(job, context.Resolve<IAppDomain>(), context.Resolve<ISyncLog>()), syncJob);

			builder.RegisterInstance(new ContextLogger(correlationId, logger)).As<ISyncLog>();
			builder.RegisterInstance(syncJobParameters).As<SyncJobParameters>();
			builder.RegisterInstance(correlationId).As<CorrelationId>();
			builder.RegisterInstance(configuration).As<SyncConfiguration>();
			builder.RegisterType<SyncExecutionContextFactory>().As<ISyncExecutionContextFactory>();
			builder.RegisterType<AppDomainWrapper>().As<IAppDomain>();

			_pipelineBuilder.RegisterFlow(builder);

			const string command = "command";
			builder.RegisterGeneric(typeof(Command<>)).Named(command, typeof(ICommand<>));
			builder.RegisterGenericDecorator(typeof(CommandWithMetrics<>), typeof(ICommand<>), command);

			// Register dependencies from installers. These generally register new types
			// but may also override registrations performed immediately above.
			foreach (IInstaller installer in installers)
			{
				installer.Install(builder);
			}
		}
	}
}