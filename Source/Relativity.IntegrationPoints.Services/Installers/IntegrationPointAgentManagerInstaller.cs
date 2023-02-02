using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.IntegrationPoints.Domain.Managers;
using Relativity.API;

namespace Relativity.IntegrationPoints.Services.Installers
{
    public class IntegrationPointAgentManagerInstaller : Installer
    {
        protected override IList<IWindsorInstaller> Dependencies { get; } = new List<IWindsorInstaller>
        {
            new SharedAgentInstaller()
        };

        protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
        {
            container.Register(Component.For<IInstanceSettingsManager>().ImplementedBy<InstanceSettingsManager>().LifestyleTransient());
            container.Register(Component.For<IRepositoryFactory>().UsingFactoryMethod(c => new RepositoryFactory(c.Resolve<IHelper>(), c.Resolve<IHelper>().GetServicesManager())));
            container.Register(Component.For<IKubernetesMode>().ImplementedBy<KubernetesMode>().LifestyleSingleton());
            container.Register(Component.For<IRelativityObjectManagerFactory>().ImplementedBy<RelativityObjectManagerFactory>());
        }
    }
}