using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Relativity.API;
using Relativity.Core.Service;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web.RelativityServices
{
	public static class RelativityServicesInstaler
	{
		public static IWindsorContainer AddRelativityServices(this IWindsorContainer container)
		{
			container.Register(Component
				.For<ICPHelper, IHelper>()
				.UsingFactoryMethod(k => new RetriableCPHelperProxy(ConnectionHelper.Helper()))
				.LifestyleTransient()
			);
			container.Register(Component
				.For<IHtmlSanitizerManager>()
				.ImplementedBy<HtmlSanitizerManager>()
				.LifestyleSingleton()
			);
			return container;
		}
	}
}