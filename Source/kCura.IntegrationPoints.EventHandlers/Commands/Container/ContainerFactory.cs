using Castle.Windsor;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Container
{
	public class ContainerFactory : IContainerFactory
	{
		public IWindsorContainer Create(IEHHelper helper)
		{
			var container = new WindsorContainer();
			container.Install(new EventHandlerInstaller(helper));
			return container;
		}
	}
}