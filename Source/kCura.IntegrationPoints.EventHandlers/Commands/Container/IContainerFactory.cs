using Castle.Windsor;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Container
{
    public interface IContainerFactory
    {
        IWindsorContainer Create(IEHContext context);
    }
}