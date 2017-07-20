using Castle.Windsor;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Container
{
	public interface IContainerFactory
	{
		IWindsorContainer Create(IEHHelper helper);
	}
}