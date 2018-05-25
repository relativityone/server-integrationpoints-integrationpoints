using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Installers
{
    public class HealthCheckInstaller : Installer
    {
        private readonly List<IWindsorInstaller> _dependencies;

        public HealthCheckInstaller()
        {
            _dependencies = new List<IWindsorInstaller>
            {
                new QueryInstallers(),
                new SharedAgentInstaller(),
                new ServicesInstaller()
            };
        }

        protected override IList<IWindsorInstaller> Dependencies => _dependencies;

        protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceId)
        {
            container.Register(Component.For<IServiceHelper>().UsingFactoryMethod(k => global::Relativity.API.Services.Helper, true));
            container.Register(Component.For<IHelper>().UsingFactoryMethod(k => k.Resolve<IServiceHelper>(), true));
            container.Register(Component.For<IRelativityManagerSoapFactory>().ImplementedBy<RelativityManagerSoapFactory>().LifestyleTransient());
        }
    }
}
