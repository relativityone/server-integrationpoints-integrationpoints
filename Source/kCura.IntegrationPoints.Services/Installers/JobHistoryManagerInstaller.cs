using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Services.Installers.Authentication;
using kCura.IntegrationPoints.Services.Installers.Context;
using kCura.IntegrationPoints.Services.JobHistory;
using Relativity.API;
using System.Collections.Generic;
using Castle.MicroKernel;
using kCura.IntegrationPoints.Data.Repositories;
using IJobHistoryRepository = kCura.IntegrationPoints.Services.Repositories.IJobHistoryRepository;
using JobHistoryRepository = kCura.IntegrationPoints.Services.Repositories.Implementations.JobHistoryRepository;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class JobHistoryManagerInstaller : Installer
	{
		private readonly List<IWindsorInstaller> _dependencies;

		public JobHistoryManagerInstaller()
		{
			_dependencies = new List<IWindsorInstaller>
			{
				new QueryInstallers(),
				new SharedAgentInstaller(),
				new ServicesInstaller()
			};
		}

		protected override IList<IWindsorInstaller> Dependencies => _dependencies;

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
		{
			container.Register(Component.For<IDestinationParser>().ImplementedBy<DestinationParser>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryAccess>().ImplementedBy<JobHistoryAccess>().LifestyleTransient());
			container.Register(Component.For<IJobHistorySummaryModelBuilder>().ImplementedBy<JobHistorySummaryModelBuilder>().LifestyleTransient());
			container.Register(Component.For<ILibraryFactory>().ImplementedBy<LibraryFactory>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryRepository>().ImplementedBy<JobHistoryRepository>().LifestyleTransient());
			container.Register(Component
				.For<IRelativityIntegrationPointsRepository>()
				.ImplementedBy<RelativityIntegrationPointsRepositoryAdminAccess>()
				.UsingFactoryMethod(k => CreateRelativityIntegrationPointsRepository(k, workspaceID))
				.LifestyleTransient()
			);
			container.Register(Component.For<ICompletedJobsHistoryRepository>().ImplementedBy<CompletedJobsHistoryRepository>().LifestyleTransient());
			container.Register(Component.For<IRsapiClientWithWorkspaceFactory>().ImplementedBy<RsapiClientWithWorkspaceFactory>());

			container
				.AddWorkspaceContext(workspaceID)
				.AddAuthTokenGenerator();
		}

		private IRelativityIntegrationPointsRepository CreateRelativityIntegrationPointsRepository(IKernel k, int workspaceID)
		{
			var rsapiAdminAccess = new RSAPIServiceAdminAccess(k.Resolve<IHelper>(), workspaceID);
			IIntegrationPointRepository integrationPointRepository = k.Resolve<IIntegrationPointRepository>();

			return new RelativityIntegrationPointsRepositoryAdminAccess(
				rsapiAdminAccess,
				integrationPointRepository
			);
		}
	}
}