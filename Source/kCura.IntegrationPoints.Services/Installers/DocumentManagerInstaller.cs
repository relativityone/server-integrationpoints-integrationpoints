using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Services.Repositories;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class DocumentManagerInstaller : IInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{
			container.Register(Component.For<IDocumentRepository>().ImplementedBy<DocumentRepository>().LifestyleTransient());
		}
	}
}