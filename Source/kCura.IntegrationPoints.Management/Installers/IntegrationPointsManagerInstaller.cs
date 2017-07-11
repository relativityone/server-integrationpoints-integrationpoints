using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
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
			container.Register(Component.For<IAPILog>().UsingFactoryMethod(k => _helper.GetLoggerFactory().GetLogger()).LifestyleSingleton());
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => _helper).LifestyleSingleton());

			container.Register(Component.For<IIntegrationPointsManager>().ImplementedBy<IntegrationPointsManager>().LifestyleTransient());
			container.Register(Component.For<IApplicationRepository>().ImplementedBy<ApplicationRepository>().LifestyleTransient());

			container.Register(Component.For<IJobsWithInvalidStatus>().ImplementedBy<JobsWithInvalidStatus>().LifestyleTransient());

			container.Register(Classes.FromAssembly(typeof(IntegrationPointsManagerInstaller).Assembly).BasedOn<IManagementTask>().WithService.FromInterface().LifestyleTransient());
		}
	}
}