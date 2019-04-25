using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Domain.Authentication;

namespace kCura.IntegrationPoints.Services.Installers.Authentication
{
    internal static class AuthTokenRegistration
    {
        /// <summary>
        /// Registers <see cref="IAuthTokenGenerator"/>
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IWindsorContainer AddAuthTokenGenerator(this IWindsorContainer container)
        {
            container.Register(
                Component
                    .For<IAuthTokenGenerator>()
                    .ImplementedBy<ClaimsTokenGenerator>()
                    .LifestyleTransient()
            );

            return container;
        }
    }
}
