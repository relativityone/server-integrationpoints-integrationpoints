using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Data.Installers
{
	public class QueryInstallers : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<Queries.CreateErrorRdo>().ImplementedBy<Queries.CreateErrorRdo>().LifestyleTransient());
			container.Register(Component.For<RelativityRdoQuery>().ImplementedBy<RelativityRdoQuery>().LifestyleTransient());
			container.Register(Component.For<ChoiceQuery>().ImplementedBy<ChoiceQuery>().LifeStyle.Transient);
		}
	}
}
