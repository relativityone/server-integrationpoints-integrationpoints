using System;
using Autofac;
using Banzai.Logging;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;

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
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, IRelativityServices relativityServices)
		{
			return Create(container, syncJobParameters, relativityServices, new SyncJobExecutionConfiguration(), new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, ISyncLog logger)
		{
			return Create(container, syncJobParameters, relativityServices, new SyncJobExecutionConfiguration(), logger);
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration configuration)
		{
			return Create(container, syncJobParameters, relativityServices, configuration, new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration configuration, ISyncLog logger)
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

            ServiceFactoryForAdminFactory serviceFactoryForAdminFactory =
                new ServiceFactoryForAdminFactory(relativityServices.ServicesMgr, logger);
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = serviceFactoryForAdminFactory.Create();

			InstallSumMetrics(serviceFactoryForAdmin, logger);

			return new SyncJobInLifetimeScope(_containerFactory, container, syncJobParameters, relativityServices, configuration, logger);
		}

		private static void InstallSumMetrics(ISourceServiceFactoryForAdmin servicesMgr, ISyncLog logger)
		{
			ITelemetryManager telemetryManager = new TelemetryMetricsInstaller(servicesMgr, logger);

			// Telemetry providers should be added here using this method: `void ITelemetryManager.AddMetricProvider(ITelemetryMetricProvider metricProvider)`
			telemetryManager.AddMetricProvider(new MainTelemetryMetricsProvider(logger));
			telemetryManager.AddMetricProvider(new KeplerTelemetryMetricsProvider(logger));

			telemetryManager.InstallMetrics().ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}