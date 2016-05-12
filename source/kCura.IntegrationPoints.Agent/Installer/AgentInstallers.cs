using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
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
			container.Register(Component.For<ExportServiceManager>().ImplementedBy<ExportServiceManager>().LifeStyle.Transient);
			container.Register(Component.For<SyncCustodianManagerWorker>().ImplementedBy<SyncCustodianManagerWorker>().LifeStyle.Transient);
			container.Register(Component.For<CreateErrorRdo>().ImplementedBy<CreateErrorRdo>().LifeStyle.Transient);
			container.Register(Component.For<ITaskFactory>().AsFactory(x => x.SelectedWith(new TaskComponentSelector())).LifeStyle.Transient);
			if (container.Kernel.HasComponent(typeof (Apps.Common.Utils.Serializers.ISerializer)) == false)
			{
				container.Register(Component.For<kCura.Apps.Common.Utils.Serializers.ISerializer>().ImplementedBy<kCura.Apps.Common.Utils.Serializers.JSONSerializer>().LifestyleTransient());
			}
			container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(f => f.Resolve<IHelper>().GetServicesManager()));
			container.Register(Component.For<IPermissionService>().ImplementedBy<PermissionService>().LifestyleTransient());

			container.Register(Component.For<SendEmailManager>().ImplementedBy<SendEmailManager>().LifeStyle.Transient);
			container.Register(Component.For<SendEmailWorker>().ImplementedBy<SendEmailWorker>().LifeStyle.Transient);
			container.Register(Component.For<JobStatisticsService>().ImplementedBy<JobStatisticsService>().LifeStyle.Transient);

            container.Register(Component.For<ExportManager>().ImplementedBy<ExportManager>().LifeStyle.Transient);
            container.Register(Component.For<ExportWorker>().ImplementedBy<ExportWorker>()
				.DependsOn(Dependency.OnComponent<ISynchronizerFactory, ExportDestinationSynchronizerFactory>())
				.LifeStyle.Transient);
		}
	}
}