using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Context.UserContext;

namespace kCura.IntegrationPoints.Web.Installers.Context
{
    internal static class UserContextRegistration
    {
        /// <summary>
        /// Registers <see cref="IUserContext"/> in container
        /// </summary>
        public static IWindsorContainer AddUserContext(this IWindsorContainer container)
        {
            return container.Register(
                Component
                    .For<IUserContext>()
                    .ImplementedBy<RequestHeadersUserContextService>()
                    .LifestylePerWebRequest(),
                Component
                    .For<IUserContext>()
                    .ImplementedBy<SessionUserContextService>()
                    .LifestylePerWebRequest(),
                Component
                    .For<IUserContext>()
                    .ImplementedBy<NotFoundUserContextService>()
                    .LifestyleSingleton()
            );
        }
    }
}
