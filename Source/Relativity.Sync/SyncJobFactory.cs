using System;
using Autofac;

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

			using (ILifetimeScope scope = container.BeginLifetimeScope(builder => RegisterDependencies(builder, syncJobParameters, configuration, logger)))
			{
				return scope.Resolve<ISyncJob>();
			}
		}

		private void RegisterDependencies(ContainerBuilder builder, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger)
		{
			CorrelationId correlationId = new CorrelationId(syncJobParameters.CorrelationId);
			builder.RegisterType<SyncJob>().As<ISyncJob>();
			builder.RegisterInstance(new ContextLogger(correlationId, logger)).As<ISyncLog>();
			builder.RegisterInstance(syncJobParameters).As<SyncJobParameters>();
			builder.RegisterInstance(configuration).As<SyncConfiguration>();
			builder.RegisterType<SyncExecutionContextFactory>().As<ISyncExecutionContextFactory>();

			_pipelineBuilder.RegisterFlow(builder);

			builder.RegisterGeneric(typeof(Command<>)).As(typeof(ICommand<>));
		}
	}
}