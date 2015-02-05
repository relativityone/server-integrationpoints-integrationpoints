using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Installer
{
	public class AgentInstallers : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Kernel.AddFacility<TypedFactoryFacility>();

			container.Register(Component.For<SyncManager>().ImplementedBy<SyncManager>().LifeStyle.Transient);
			container.Register(Component.For<SyncWorker>().ImplementedBy<SyncWorker>().LifeStyle.Transient);
			container.Register(Component.For<RdoSynchronizer>().ImplementedBy<RdoSynchronizer>().LifeStyle.Transient);
			container.Register(Component.For<CreateErrorRDO>().ImplementedBy<CreateErrorRDO>().LifeStyle.Transient);
			container.Register(Component.For<ITaskFactory>().AsFactory(x => x.SelectedWith(new TaskComponentSelector())).LifeStyle.Transient);
			container.Register(Component.For<kCura.Apps.Common.Utils.Serializers.ISerializer>().ImplementedBy<kCura.Apps.Common.Utils.Serializers.JSONSerializer>().LifestyleTransient());
			}
	}
}
