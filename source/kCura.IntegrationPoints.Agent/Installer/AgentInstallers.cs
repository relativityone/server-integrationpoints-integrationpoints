using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;

namespace kCura.IntegrationPoints.Agent.Installer
{
	public class AgentInstallers : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IServiceContext>().ImplementedBy<ServiceContext>().LifestyleTransient());
			container.Register(Component.For<SyncManager>().ImplementedBy<SyncManager>().LifeStyle.Transient);
			container.Register(Component.For<SyncWorker>().ImplementedBy<SyncWorker>().LifeStyle.Transient);
			container.Register(Component.For<ITaskFactory>().AsFactory(x => x.SelectedWith<TaskComponentSelector>()).LifeStyle.Transient);
		}
	}
}
