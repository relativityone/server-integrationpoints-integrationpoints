using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Container
{
	public class ContainerFactory : IContainerFactory
	{
		public IWindsorContainer Create(IEHContext context)
		{
			var container = new WindsorContainer();
			container.Install(new EventHandlerInstaller(context));
			container.Install(new QueryInstallers());
			container.Install(new ServicesInstaller());
			return container;
		}
	}
}