using System;
using Autofac;
using Banzai.Logging;
using Relativity.API;
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
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, RelativityServices relativityServices)
		{
			return Create(container, syncJobParameters, relativityServices, new SyncJobExecutionConfiguration(), new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, RelativityServices relativityServices, ISyncLog logger)
		{
			return Create(container, syncJobParameters, relativityServices, new SyncJobExecutionConfiguration(), logger);
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, RelativityServices relativityServices, SyncJobExecutionConfiguration configuration)
		{
			return Create(container, syncJobParameters, relativityServices, configuration, new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, RelativityServices relativityServices, SyncJobExecutionConfiguration configuration, ISyncLog logger)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			if (syncJobParameters == null)
			{
				throw new ArgumentNullException(nameof(syncJobParameters));
			}

			if (relativityServices == null)
			{
				throw new ArgumentNullException(nameof(relativityServices));
			}

			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			LogWriter.SetFactory(new SyncLogWriterFactory(logger));

			InstallSumMetrics(relativityServices.ServicesMgr, logger);

			return new SyncJobInLifetimeScope(_containerFactory, container, syncJobParameters, relativityServices, configuration, logger);
		}

		private void InstallSumMetrics(IServicesMgr servicesMgr, ISyncLog logger)
		{
			ITelemetryManager telemetryManager = new TelemetryManager(servicesMgr, logger);

			telemetryManager.AddMetricProviders(new TelemetryMetricsProvider(servicesMgr, logger));

			telemetryManager.InstallMetrics();
		}
	}
}