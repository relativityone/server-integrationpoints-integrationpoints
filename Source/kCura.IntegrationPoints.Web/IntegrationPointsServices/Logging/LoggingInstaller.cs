using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging
{
	internal static class LoggingInstaller
	{
		public static IWindsorContainer AddLoggingContext(this IWindsorContainer container)
		{
			container.Register(Component.For<ICacheHolder>().ImplementedBy<CacheHolder>().LifestyleSingleton());
			container.Register(Component.For<IWebCorrelationContextProvider>().ImplementedBy<WebActionContextProvider>().LifestyleTransient());
			return container;
		}
	}
}