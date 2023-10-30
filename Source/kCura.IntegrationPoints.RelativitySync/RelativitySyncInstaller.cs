﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.RelativitySync;

namespace kCura.IntegrationPoints.RelativitySync
{
    public class RelativitySyncInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IExtendedJob>().ImplementedBy<ExtendedJob>());
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.Register(Component.For<ICancellationAdapter>().ImplementedBy<CancellationAdapter>());
            container.Register(Component.For<IIntegrationPointToSyncConverter, IIntegrationPointToSyncAppConverter>().ImplementedBy<IntegrationPointToSyncConverter>().LifestyleTransient());
            container.Register(Component.For<ISyncOperationsWrapper>().ImplementedBy<SyncOperationsWrapper>().LifestyleTransient().Named(nameof(SyncOperationsWrapper)));
            container.Register(Component.For<IJobHistorySyncService>().ImplementedBy<JobHistorySyncService>().LifestyleTransient());
            container.Register(Component.For<ISyncFieldMapConverter>().ImplementedBy<SyncFieldMapConverter>().LifestyleTransient());
        }
    }
}
