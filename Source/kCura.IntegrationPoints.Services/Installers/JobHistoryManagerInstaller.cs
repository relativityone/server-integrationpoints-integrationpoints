using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
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
                new QueryInstallers(),
				new SharedAgentInstaller(),
                new ServicesInstaller()
            };
        }

		protected override IList<IWindsorInstaller> Dependencies => _dependencies;

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{
			container.Register(Component.For<IServiceHelper>().UsingFactoryMethod(k => global::Relativity.API.Services.Helper, true));
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => k.Resolve<IServiceHelper>(), true));
			container.Register(Component.For<IDestinationParser>().ImplementedBy<DestinationParser>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryAccess>().ImplementedBy<JobHistoryAccess>().LifestyleTransient());
			container.Register(Component.For<IJobHistorySummaryModelBuilder>().ImplementedBy<JobHistorySummaryModelBuilder>().LifestyleTransient());
			container.Register(Component.For<ILibraryFactory>().ImplementedBy<LibraryFactory>().LifestyleTransient());
			container.Register(Component.For<IJobHistoryRepository>().ImplementedBy<JobHistoryRepository>().LifestyleTransient());
			container.Register(Component.For<RelativityIntegrationPointsRepositoryAdminAccess>().ImplementedBy<RelativityIntegrationPointsRepositoryAdminAccess>().LifestyleTransient());
			container.Register(
				Component.For<IRelativityIntegrationPointsRepository>()
					.UsingFactoryMethod(k => k.Resolve<RelativityIntegrationPointsRepositoryAdminAccess>(new RSAPIServiceAdminAccess(k.Resolve<IHelper>(), workspaceId)))
					.LifestyleTransient());
			container.Register(Component.For<ICompletedJobsHistoryRepository>().ImplementedBy<CompletedJobsHistoryRepository>().LifestyleTransient());
			container.Register(Component.For<IRSAPIService>().UsingFactoryMethod(k => ServiceContextFactory.CreateRSAPIService(k.Resolve<IHelper>(), workspaceId), true));
			container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());

			container.Register(Component.For<IRsapiClientWithWorkspaceFactory>().ImplementedBy<RsapiClientWithWorkspaceFactory>());

			container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IServiceHelper helper = k.Resolve<IServiceHelper>();
					var rsapiClientFactory = k.Resolve<IRsapiClientWithWorkspaceFactory>();
					return new ServiceContextHelperForKeplerService(helper, workspaceId, rsapiClientFactory);
				}));
		}
	}
}