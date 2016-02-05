using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.ScheduleQueue.Core.Logging;

namespace kCura.IntegrationPoints.Agent.Installer
{
	public class AgentInstallers : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Kernel.AddFacility<TypedFactoryFacility>();

			container.Register(Component.For<SyncManager>().ImplementedBy<SyncManager>().LifeStyle.Transient);
			container.Register(Component.For<SyncWorker>().ImplementedBy<SyncWorker>().LifeStyle.Transient);
			container.Register(Component.For<SyncCustodianManagerWorker>().ImplementedBy<SyncCustodianManagerWorker>().LifeStyle.Transient);
			container.Register(Component.For<CreateErrorRDO>().ImplementedBy<CreateErrorRDO>().LifeStyle.Transient);
			container.Register(Component.For<ITaskFactory>().AsFactory(x => x.SelectedWith(new TaskComponentSelector())).LifeStyle.Transient);
			container.Register(Component.For<kCura.Apps.Common.Utils.Serializers.ISerializer>().ImplementedBy<kCura.Apps.Common.Utils.Serializers.JSONSerializer>().LifestyleTransient());

			container.Register(Component.For<SendEmailManager>().ImplementedBy<SendEmailManager>().LifeStyle.Transient);
			container.Register(Component.For<SendEmailWorker>().ImplementedBy<SendEmailWorker>().LifeStyle.Transient);
			container.Register(Component.For<JobStatisticsService>().ImplementedBy<JobStatisticsService>().LifeStyle.Transient);
		}
	}
}
