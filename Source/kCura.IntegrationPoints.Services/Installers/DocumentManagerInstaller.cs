using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Services.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class DocumentManagerInstaller : Installer
	{
		public DocumentManagerInstaller()
		{
			Dependencies = new List<IWindsorInstaller>();
		}

		protected override IList<IWindsorInstaller> Dependencies { get; }

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{
			container.Register(Component.For<IDocumentRepository>().ImplementedBy<DocumentRepository>().LifestyleTransient());
		}
	}
}