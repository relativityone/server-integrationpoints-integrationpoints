using System;
using Autofac;
using Banzai.Logging;
using Relativity.Sync.Logging;

namespace Relativity.Sync
{
	/// <inheritdoc />
	public sealed class SyncJobFactory : ISyncJobFactory
	{
		private readonly IContainerFactory _containerFactory;

		/// <inheritdoc />
		public SyncJobFactory() : this(new ContainerFactory())
		{
		}

		internal SyncJobFactory(IContainerFactory containerFactory)
		{
			_containerFactory = containerFactory;
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters)
		{
			return Create(container, syncJobParameters, new SyncJobExecutionConfiguration(), new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, ISyncLog logger)
		{
			return Create(container, syncJobParameters, new SyncJobExecutionConfiguration(), logger);
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncJobExecutionConfiguration configuration)
		{
			return Create(container, syncJobParameters, configuration, new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncJobExecutionConfiguration configuration, ISyncLog logger)
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

				using (ILifetimeScope scope = container.BeginLifetimeScope(builder => _containerFactory.RegisterSyncDependencies(builder, syncJobParameters, configuration, logger)))
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
	}
}