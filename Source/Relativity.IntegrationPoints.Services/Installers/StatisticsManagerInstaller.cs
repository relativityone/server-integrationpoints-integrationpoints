using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data.Installers;
using Relativity.IntegrationPoints.Services.Installers.Authentication;
using Relativity.IntegrationPoints.Services.Installers.Context;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.Installers
{
    public class StatisticsManagerInstaller : Installer
    {
        private readonly List<IWindsorInstaller> _dependencies;

        public StatisticsManagerInstaller()
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
            container
                .AddWorkspaceContext(workspaceID)
                .AddAuthTokenGenerator();
        }
    }
}
