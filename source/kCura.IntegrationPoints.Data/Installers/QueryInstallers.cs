using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Data.Installers
{
	public class QueryInstallers : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<CreateErrorRdo>().ImplementedBy<CreateErrorRdo>().LifestyleTransient());
			container.Register(Component.For<IObjectTypeQuery>().ImplementedBy<SqlObjectTypeQuery>().LifestyleTransient());
			container.Register(Component.For<RSAPIRdoQuery>().ImplementedBy<RSAPIRdoQuery>().LifeStyle.Transient);
			container.Register(Component.For<IChoiceQuery>().ImplementedBy<ChoiceQuery>().LifeStyle.Transient);
			container.Register(Component.For<JobHistoryError>().ImplementedBy<JobHistoryError>().LifeStyle.Transient);
			container.Register(Component.For<JobResourceTracker>().ImplementedBy<JobResourceTracker>().LifeStyle.Transient);
			container.Register(Component.For<JobStatisticsQuery>().ImplementedBy<JobStatisticsQuery>().LifeStyle.Transient);

            container.Register(Component.For<IFileQuery>().ImplementedBy<FileQuery>().LifeStyle.Transient);
        }
    }
}
