using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;

namespace kCura.IntegrationPoints.Data.Installers
{
    public static class RetryingMechanismRegistration
    {
        public static IWindsorContainer AddRetryingMechanism(this IWindsorContainer container)
        {
            container.Register(Component
                .For<IRetryHandlerFactory>()
                .ImplementedBy<RetryHandlerFactory>()
                .LifestyleSingleton());

            container.Register(Component
                .For<IRetryHandler>()
                .ImplementedBy<RetryHandler>()
                .LifestyleSingleton());

            return container;
        }
    }
}
