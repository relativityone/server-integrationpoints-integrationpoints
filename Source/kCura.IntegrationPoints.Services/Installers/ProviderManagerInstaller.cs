using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Services.Installers.Context;
using kCura.IntegrationPoints.Services.Installers.ProviderManager;
using kCura.IntegrationPoints.Services.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using System.Collections.Generic;
using kCura.IntegrationPoints.Services.Installers.Authentication;

namespace kCura.IntegrationPoints.Services.Installers
{
    public class ProviderManagerInstaller : Installer
    {
        private readonly List<IWindsorInstaller> _dependencies;

        public ProviderManagerInstaller()
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
            container.Register(Component.For<IProviderRepository>().ImplementedBy<ProviderRepository>().LifestyleTransient());

            container
                .AddWorkspaceContext(workspaceID)
                .AddAuthTokenGenerator()
                .AddProviderInstallerAndUninstaller();
        }
    }
}