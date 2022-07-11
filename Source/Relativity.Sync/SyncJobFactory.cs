using System;
using Banzai.Logging;
using Relativity.API;
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
        public ISyncJob Create(SyncJobParameters syncJobParameters, IRelativityServices relativityServices)
        {
            return Create(syncJobParameters, relativityServices, new SyncJobExecutionConfiguration(), new EmptyLogger());
        }

        /// <inheritdoc />
        public ISyncJob Create(SyncJobParameters syncJobParameters, IRelativityServices relativityServices, IAPILog logger)
        {
            return Create(syncJobParameters, relativityServices, new SyncJobExecutionConfiguration(), logger);
        }

        /// <inheritdoc />
        public ISyncJob Create(SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration configuration)
        {
            return Create(syncJobParameters, relativityServices, configuration, new EmptyLogger());
        }

        /// <inheritdoc />
        public ISyncJob Create(SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration configuration, IAPILog logger)
        {
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

            ServiceFactoryForAdminFactory serviceFactoryForAdminFactory = new ServiceFactoryForAdminFactory(relativityServices.Helper.GetServicesManager(), logger);
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = serviceFactoryForAdminFactory.Create();

            InstallSumMetrics(serviceFactoryForAdmin, logger);

            return new SyncJobInLifetimeScope(_containerFactory, syncJobParameters, relativityServices, configuration, logger);
        }

        private static void InstallSumMetrics(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger)
        {
            ITelemetryManager telemetryManager = new TelemetryMetricsInstaller(serviceFactoryForAdmin, logger);

            // Telemetry providers should be added here using this method: `void ITelemetryManager.AddMetricProvider(ITelemetryMetricProvider metricProvider)`
            telemetryManager.AddMetricProvider(new MainTelemetryMetricsProvider(logger));
            telemetryManager.AddMetricProvider(new KeplerTelemetryMetricsProvider(logger));

            telemetryManager.InstallMetrics().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}