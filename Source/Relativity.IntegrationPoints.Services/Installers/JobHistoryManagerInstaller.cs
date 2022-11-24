using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using Relativity.IntegrationPoints.Services.Installers.Authentication;
using Relativity.IntegrationPoints.Services.Installers.Context;
using Relativity.API;
using System.Collections.Generic;
using Castle.MicroKernel;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.RelativitySync;
using Relativity.IntegrationPoints.Services.JobHistory;

namespace Relativity.IntegrationPoints.Services.Installers
{
    public class JobHistoryManagerInstaller : Installer
    {
        private readonly List<IWindsorInstaller> _dependencies;

        public JobHistoryManagerInstaller()
        {
            _dependencies = new List<IWindsorInstaller>
            {
                new QueryInstallers(),
                new SharedAgentInstaller(),
                new ServicesInstaller(),
                new ValidationInstaller(),
                new RelativitySyncInstaller(),
                new kCura.IntegrationPoints.ImportProvider.Parser.Installers.ServicesInstaller(),
            };
        }

        protected override IList<IWindsorInstaller> Dependencies => _dependencies;

        protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
        {
            container.Register(Component.For<IDestinationParser>().ImplementedBy<DestinationParser>().LifestyleTransient());
            container.Register(Component.For<IJobHistoryAccess>().ImplementedBy<JobHistoryAccess>().LifestyleTransient());
            container.Register(Component.For<IJobHistorySummaryModelBuilder>().ImplementedBy<JobHistorySummaryModelBuilder>().LifestyleTransient());
            container.Register(Component.For<Repositories.IJobHistoryAccessor>().ImplementedBy<Repositories.Implementations.JobHistoryAccessor>().LifestyleTransient());
            container.Register(Component
                .For<IRelativityIntegrationPointsRepository>()
                .ImplementedBy<RelativityIntegrationPointsRepositoryAdminAccess>()
                .UsingFactoryMethod(k => CreateRelativityIntegrationPointsRepository(k, workspaceID))
                .LifestyleTransient()
            );
            container.Register(Component.For<ICompletedJobsHistoryRepository>().ImplementedBy<CompletedJobsHistoryRepository>().LifestyleTransient());

            container
                .AddWorkspaceContext(workspaceID)
                .AddAuthTokenGenerator();
        }

        private IRelativityIntegrationPointsRepository CreateRelativityIntegrationPointsRepository(IKernel k, int workspaceID)
        {
            var relativityObjectManagerServiceAdminAccess = new RelativityObjectManagerServiceAdminAccess(k.Resolve<IHelper>(), workspaceID);
            IIntegrationPointService integrationPointService = k.Resolve<IIntegrationPointService>();

            return new RelativityIntegrationPointsRepositoryAdminAccess(
                relativityObjectManagerServiceAdminAccess,
                integrationPointService
            );
        }
    }
}
