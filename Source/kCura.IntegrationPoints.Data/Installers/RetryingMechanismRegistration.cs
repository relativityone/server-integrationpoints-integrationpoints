using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Common;

namespace kCura.IntegrationPoints.Data.Installers
{
    public static class RetryingMechanismRegistration
    {
        public static IWindsorContainer AddRetryingMechanism(this IWindsorContainer container)
        {
            return container.Register(
                Component
                    .For<IRetryHandlerFactory>()
                    .ImplementedBy<RetryHandlerFactory>()
                    .LifestyleSingleton()
            );
        }
    }
}
