using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Installers
{
	public class DocumentTransferProviderInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IExtendedImportApiFactory>().ImplementedBy<ExtendedImportApiFactory>().LifestyleSingleton());
		}
	}
}
