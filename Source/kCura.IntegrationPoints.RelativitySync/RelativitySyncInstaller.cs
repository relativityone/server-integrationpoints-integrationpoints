﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.RelativitySync.Metrics;
using System;

namespace kCura.IntegrationPoints.RelativitySync
{
    public class RelativitySyncInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IExtendedJob>().ImplementedBy<ExtendedJob>());
            container.Register(Component.For<RelativitySyncAdapter>().ImplementedBy<RelativitySyncAdapter>());
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.Register(Component.For<ICancellationAdapter>().ImplementedBy<CancellationAdapter>());
            container.Register(Component.For<IIntegrationPointToSyncConverter>().ImplementedBy<IntegrationPointToSyncConverter>());
            container.Register(Component.For<IMetricsFactory>().ImplementedBy<MetricsFactory>().LifestyleTransient());
            container.Register(Component.For<ISyncOperationsWrapper>().ImplementedBy<SyncOperationsWrapper>().LifestyleTransient()
                .Named(nameof(SyncOperationsWrapper)));
            container.Register(Component.For<ISyncConfigurationService>().ImplementedBy<SyncConfigurationService>().LifestyleTransient());
            container.Register(Component.For<ISyncJobMetric>().ImplementedBy<SyncJobMetric>().LifestyleTransient());
            container.Register(Component.For<IJobHistorySyncService>().ImplementedBy<JobHistorySyncService>().LifestyleTransient());
        }
    }
}
