using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Helpers;

namespace kCura.IntegrationPoints.Web.Installers
{
	public static class HelpersRegistration
	{
		public static IWindsorContainer AddHelpers(this IWindsorContainer container)
		{
			return container.Register(
				Component
					.For<IRelativityUrlHelper>()
					.ImplementedBy<RelativityUrlHelper>()
					.LifestyleTransient(),
				Component
					.For<SummaryPageSelector>()
					.LifestyleSingleton()
			);
		}
	}
}