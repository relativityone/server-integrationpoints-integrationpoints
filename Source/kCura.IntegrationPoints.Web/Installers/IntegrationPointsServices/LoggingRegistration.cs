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
			container.Register(Component.For<ICacheHolder>().ImplementedBy<CacheHolder>().LifestyleSingleton());
			container.Register(Component.For<IWebCorrelationContextProvider>().ImplementedBy<WebActionContextProvider>().LifestyleTransient());
			return container;
		}
	}
}