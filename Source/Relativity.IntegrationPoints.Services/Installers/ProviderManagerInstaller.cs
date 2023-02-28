using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data.Installers;
using Relativity.IntegrationPoints.Services.Installers.Context;
using Relativity.IntegrationPoints.Services.Installers.ProviderManager;
using System.Collections.Generic;
using Relativity.IntegrationPoints.Services.Installers.Authentication;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;

namespace Relativity.IntegrationPoints.Services.Installers
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
            container.Register(Component.For<IProviderAccessor>().ImplementedBy<ProviderAccessor>().LifestyleTransient());

            container
                .AddWorkspaceContext(workspaceID)
                .AddAuthTokenGenerator()
                .AddProviderInstallerAndUninstaller();
        }
    }
}
