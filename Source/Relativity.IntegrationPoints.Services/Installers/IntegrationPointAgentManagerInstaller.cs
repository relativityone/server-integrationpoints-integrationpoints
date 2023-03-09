using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;

namespace Relativity.IntegrationPoints.Services.Installers
{
    public class IntegrationPointAgentManagerInstaller : Installer
    {
        protected override IList<IWindsorInstaller> Dependencies => new List<IWindsorInstaller>
        {
            new SharedAgentInstaller(),
            new ServicesInstaller()
        };

        protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
        {
        }
    }
}
