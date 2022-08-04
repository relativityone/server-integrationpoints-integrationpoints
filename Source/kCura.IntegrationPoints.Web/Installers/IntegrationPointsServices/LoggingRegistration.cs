using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging;

namespace kCura.IntegrationPoints.Web.Installers.IntegrationPointsServices
{
    internal static class LoggingRegistration
    {
        /// <summary>
        /// Registers <see cref="IWebCorrelationContextProvider"/> in a container.
        /// </summary>
        public static IWindsorContainer AddLoggingContext(this IWindsorContainer container)
        {
            return container.Register(
                Component
                    .For<ICacheHolder>()
                    .ImplementedBy<CacheHolder>()
                    .LifestyleSingleton(),
                Component
                    .For<IWebCorrelationContextProvider>()
                    .ImplementedBy<WebActionContextProvider>()
                    .LifestylePerWebRequest()
            );
        }
    }
}