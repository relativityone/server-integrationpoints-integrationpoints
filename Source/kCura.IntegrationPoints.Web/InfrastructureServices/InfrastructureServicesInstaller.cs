using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.InfrastructureServices
{
	internal static class InfrastructureServicesInstaller
	{
		public static IWindsorContainer AddServices(this IWindsorContainer container)
		{
			container.Register(Component
				.For<ISessionService>()
				.UsingFactoryMethod(k => SessionServiceFactory.GetSessionService(k.Resolve<ICPHelper>))
				.LifestylePerWebRequest());

			return container;
		}
	}
}