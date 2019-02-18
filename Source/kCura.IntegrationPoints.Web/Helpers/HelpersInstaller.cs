using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Web.Helpers
{
	public class HelpersInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IRelativityUrlHelper>().ImplementedBy<RelativityUrlHelper>().LifestyleTransient());
			container.Register(Component.For<SummaryPageSelector>().ImplementedBy<SummaryPageSelector>().LifestyleSingleton());
		}
	}
}