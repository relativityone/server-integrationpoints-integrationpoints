using System;
using Autofac;
using Banzai.Logging;
using Relativity.API;
using Relativity.Sync.Authentication;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;

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
			return Create(container, syncJobParameters, new SyncConfiguration(), new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, ISyncLog logger)
		{
			return Create(container, syncJobParameters, new SyncConfiguration(), logger);
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncConfiguration configuration)
		{
			return Create(container, syncJobParameters, configuration, new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
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

				using (ILifetimeScope scope = container.BeginLifetimeScope(builder => RegisterDependencies(builder, syncJobParameters, configuration, logger)))
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

		private void RegisterDependencies(ContainerBuilder builder, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger)
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
			builder.RegisterType<SystemStopwatch>().As<IStopwatch>();
			builder.RegisterType<AppDomainWrapper>().As<IAppDomain>();
			builder.RegisterType<OAuth2ClientFactory>().As<IOAuth2ClientFactory>();
			builder.RegisterType<OAuth2TokenGenerator>().As<IAuthTokenGenerator>();

			builder.RegisterType<TokenProviderFactoryFactory>().As<ITokenProviderFactoryFactory>();
			builder.RegisterType<ServiceFactoryForUser>()
				.As<ISourceServiceFactoryForUser>()
				.As<IDestinationServiceFactoryForUser>();
			builder.RegisterType<ServiceFactoryForAdmin>()
				.As<ISourceServiceFactoryForAdmin>()
				.As<IDestinationServiceFactoryForAdmin>();

			_pipelineBuilder.RegisterFlow(builder);

			const string command = "command";
			builder.RegisterGeneric(typeof(Command<>)).Named(command, typeof(ICommand<>));
			builder.RegisterGenericDecorator(typeof(CommandWithMetrics<>), typeof(ICommand<>), command);
		}
	}
}