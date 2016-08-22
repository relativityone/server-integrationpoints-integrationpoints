using System.Collections.Generic;
using System.Linq;
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
			#region Convention

			HashSet<string> queryObjectsToExclude = new HashSet<string>(
				new []
				{
					typeof (GetApplicationBinaries).Name,
					typeof (JobHistoryErrorQuery).Name,
				});
			container.Register(
				Classes.FromThisAssembly()
					.InNamespace("kCura.IntegrationPoints.Data.Queries")
					.If(x => !x.GetInterfaces().Any())
					.If(x => !queryObjectsToExclude.Contains(x.Name))
					.Configure(c => c.LifestyleTransient()));
			#endregion

//			container.Register(Component.For<CreateErrorRdo>().ImplementedBy<CreateErrorRdo>().LifestyleTransient());
//			container.Register(Component.For<JobHistoryError>().ImplementedBy<JobHistoryError>().LifeStyle.Transient);
//			container.Register(Component.For<JobResourceTracker>().ImplementedBy<JobResourceTracker>().LifeStyle.Transient);
//			container.Register(Component.For<JobStatisticsQuery>().ImplementedBy<JobStatisticsQuery>().LifeStyle.Transient);

			container.Register(Component.For<IObjectTypeQuery>().ImplementedBy<SqlObjectTypeQuery>().LifestyleTransient());
			container.Register(Component.For<RSAPIRdoQuery>().ImplementedBy<RSAPIRdoQuery>().LifeStyle.Transient);

			container.Register(Component.For<IChoiceQuery>().ImplementedBy<ChoiceQuery>().LifeStyle.Transient);
            container.Register(Component.For<IFileQuery>().ImplementedBy<FileQuery>().LifeStyle.Transient);
        }
    }
}
