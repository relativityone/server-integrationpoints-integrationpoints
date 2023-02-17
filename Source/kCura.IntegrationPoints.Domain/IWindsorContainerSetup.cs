using Castle.Windsor;
using Relativity.API;

namespace kCura.IntegrationPoints.Domain
{
    public interface IWindsorContainerSetup
    {
        IWindsorContainer SetUpCastleWindsor(IHelper helper);
    }
}
