using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;

namespace kCura.IntegrationPoints.Core.Installers.Registrations
{
    internal static class WebApiLoginServiceRegistration
    {
        public static IWindsorContainer AddWebApiLoginService(this IWindsorContainer container)
        {
            RegisterLoginHelperFacade(container);

            container.Register(Component
                .For<IWebApiLoginService>()
                .ImplementedBy<WebApiLoginService>()
                .LifestyleTransient());

            return container;
        }

        private static void RegisterLoginHelperFacade(IWindsorContainer container)
        {
            container.Register(Component
                            .For<ILoginHelperFacade>()
                            .ImplementedBy<LoginHelperRetryDecorator>()
                            .LifestyleTransient()
                        );
            container.Register(Component
                .For<ILoginHelperFacade>()
                .ImplementedBy<LoginHelperInstrumentationDecorator>()
                .LifestyleTransient()
            );
            container.Register(Component
                .For<ILoginHelperFacade>()
                .ImplementedBy<LoginHelperFacade>()
                .LifestyleSingleton()
            );
        }
    }
}
