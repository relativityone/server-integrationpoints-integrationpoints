using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Helpers;

namespace kCura.IntegrationPoints.Web.Installers
{
	public class HelpersInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IRelativityUrlHelper>().ImplementedBy<RelativityUrlHelper>().LifestyleTransient());
			container.Register(Component.For<SummaryPageSelector>().LifestyleSingleton());
		}
	}
}