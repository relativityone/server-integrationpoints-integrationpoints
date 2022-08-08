using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data.Installers;
using Relativity.IntegrationPoints.Services.Installers.Authentication;
using Relativity.IntegrationPoints.Services.Installers.Context;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;

namespace Relativity.IntegrationPoints.Services.Installers
{
    public class IntegrationPointTypeManagerInstaller : Installer
    {
        private readonly List<IWindsorInstaller> _dependencies;

        public IntegrationPointTypeManagerInstaller()
        {
            _dependencies = new List<IWindsorInstaller>
            {
                new QueryInstallers(),
                new SharedAgentInstaller(),
                new ServicesInstaller()
            };
        }

        protected override IList<IWindsorInstaller> Dependencies => _dependencies;

        protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
        {
            container.Register(Component.For<IIntegrationPointTypeRepository>().ImplementedBy<IntegrationPointTypeRepository>().LifestyleTransient());

            container
                .AddWorkspaceContext(workspaceID)
                .AddAuthTokenGenerator();
        }
    }
}