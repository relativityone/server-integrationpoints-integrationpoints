using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.WorkspaceIdProvider;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Services
{
	internal static class ServicesInstaller
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