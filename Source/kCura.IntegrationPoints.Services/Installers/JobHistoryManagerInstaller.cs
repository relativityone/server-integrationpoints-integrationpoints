using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Services.JobHistory;
using kCura.IntegrationPoints.Services.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using Relativity.API;
using Relativity.Toggles;
using Relativity.Toggles.Providers;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class JobHistoryManagerInstaller : Installer
	{
		private readonly List<IWindsorInstaller> _dependencies;

		public JobHistoryManagerInstaller()
		{
			_dependencies = new List<IWindsorInstaller>
			{
				new QueryInstallers()
			};
		}

		protected override IList<IWindsorInstaller> Dependencies => _dependencies;

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{
			container.Register(Component.For<IServiceHelper>().UsingFactoryMethod(k => global::Relativity.API.Services.Helper, true));
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => k.Resolve<IServiceHelper>(), true));
			container.Register(Component.For<ITokenProvider>().ImplementedBy<RelativityCoreTokenProvider>().LifestyleTransient());
			container.Register(Component.For<IHelperFactory>().ImplementedBy<HelperFactory>().LifestyleTransient());
			container.Register(Component.For<IManagerFactory>().ImplementedBy<ManagerFactory>().LifestyleTransient());
			container.Register(Component.For<IContextContainerFactory>().ImplementedBy<ContextContainerFactory>().LifestyleTransient());
			container.Register(Component.For<IDestinationWorkspaceParser>().ImplementedBy<DestinationWorkspaceParser>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryAccess>().ImplementedBy<JobHistoryAccess>().LifestyleTransient());
			container.Register(Component.For<IJobHistorySummaryModelBuilder>().ImplementedBy<JobHistorySummaryModelBuilder>().LifestyleTransient());
			container.Register(Component.For<ILibraryFactory>().ImplementedBy<LibraryFactory>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryRepository>().ImplementedBy<JobHistoryRepository>().LifestyleTransient());
			container.Register(Component.For<IRelativityIntegrationPointsRepository>().ImplementedBy<RelativityIntegrationPointsRepositoryAdminAccess>().LifestyleTransient());
			container.Register(Component.For<ICompletedJobsHistoryRepository>().ImplementedBy<CompletedJobsHistoryRepository>().LifestyleTransient());
			container.Register(Component.For<IRSAPIService>().UsingFactoryMethod(k => Core.Services.ServiceContext.ServiceContextFactory.CreateRSAPIService(k.Resolve<IHelper>(), workspaceId), true));
			container.Register(Component.For<IIntegrationPointSerializer>().ImplementedBy<IntegrationPointSerializer>().LifestyleSingleton());
			container.Register(Component.For<IToggleProvider>().Instance(new SqlServerToggleProvider(
				() =>
				{
					SqlConnection connection = container.Resolve<IHelper>().GetDBContext(-1).GetConnection(true);

					return connection;
				},
				async () =>
				{
					Task<SqlConnection> task = Task.Run(() =>
					{
						SqlConnection connection = container.Resolve<IHelper>().GetDBContext(-1).GetConnection(true);
						return connection;
					});

					return await task;
				})).LifestyleTransient());
		}
	}
}