using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;

namespace Relativity.IntegrationPoints.Services.Installers
{
    public class DocumentManagerInstaller : Installer
    {
        public DocumentManagerInstaller()
        {
            Dependencies = new List<IWindsorInstaller>();
        }

        protected override IList<IWindsorInstaller> Dependencies { get; }

        protected override void RegisterComponents(
            IWindsorContainer container, 
            IConfigurationStore store, 
            int workspaceID)
        {
            container.Register(Component.For<IRelativityObjectManagerFactory>()
                .ImplementedBy<RelativityObjectManagerFactory>()
                .LifestyleTransient());
            container.Register(Component.For<Repositories.IDocumentAccessor>()
                .ImplementedBy<DocumentAccessor>()
                .LifestyleTransient());
        }
    }
}