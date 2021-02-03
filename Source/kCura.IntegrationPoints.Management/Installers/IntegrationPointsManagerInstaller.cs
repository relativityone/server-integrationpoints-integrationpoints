using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Management.Tasks;
using kCura.IntegrationPoints.Management.Tasks.Helpers;
using Relativity.API;

namespace kCura.IntegrationPoints.Management.Installers
{
	public class IntegrationPointsManagerInstaller : IWindsorInstaller
	{
		private readonly IAgentHelper _helper;

		public IntegrationPointsManagerInstaller(IAgentHelper helper)
		{
			_helper = helper;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => _helper).LifestyleSingleton());

			container.Register(Component.For<IIntegrationPointsManager>().ImplementedBy<IntegrationPointsManager>().LifestyleTransient());
			container.Register(Component.For<IApplicationRepository>().ImplementedBy<ApplicationRepository>().LifestyleTransient());

			container.Register(Component.For<IJobsWithInvalidStatus>().ImplementedBy<JobsWithInvalidStatus>().LifestyleTransient());
			container.Register(Component.For<IStuckJobs>().ImplementedBy<StuckJobs>().LifestyleTransient());

			container.Register(Component.For<IRelativityObjectManagerFactory>().ImplementedBy<RelativityObjectManagerFactory>().LifestyleTransient());
			container.Register(Component.For<IRelativityObjectManager>()
				.UsingFactoryMethod(x =>
				{
					int workspaceId = x.Resolve<IServiceContextHelper>().WorkspaceID;
					IRelativityObjectManagerFactory factory = x.Resolve<IRelativityObjectManagerFactory>();
					return factory.CreateRelativityObjectManager(workspaceId);
				}).LifestyleTransient());

			container.Register(Classes.FromAssembly(typeof(IntegrationPointsManagerInstaller).Assembly).BasedOn<IManagementTask>().WithService.FromInterface().LifestyleTransient());
		}
	}
}