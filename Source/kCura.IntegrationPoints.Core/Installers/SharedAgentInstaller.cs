using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Services;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class SharedAgentInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			Guid guid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);
			container.Register(Component.For<IDeleteHistoryErrorService>().ImplementedBy<DeleteHistoryErrorService>().LifestyleTransient());
			container.Register(Component.For<IUnlinkedJobHistoryService>().ImplementedBy<UnlinkedJobHistoryService>().LifestyleTransient());
			container.Register(Component.For<IRelativityObjectManagerServiceFactory>().ImplementedBy<RelativityObjectManagerServiceFactory>().LifestyleTransient());
			container.Register(Component.For<IJobService>().ImplementedBy<JobService>().LifestyleTransient());
			container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().DependsOn(Dependency.OnValue<Guid>(guid)).LifestyleTransient());
			container.Register(Component.For<IQueueQueryManager>().ImplementedBy<QueueQueryManager>().DependsOn(Dependency.OnValue<Guid>(guid)).LifestyleTransient());
			container.Register(Component.For<IJobServiceDataProvider>().ImplementedBy<JobServiceDataProvider>().LifestyleTransient());
			container.Register(Component.For<IJobRepository>().ImplementedBy<JobRepository>().LifestyleTransient());
			container.Register(Component.For<IUnfinishedJobService>().ImplementedBy<UnfinishedJobService>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointSerializer>().ImplementedBy<IntegrationPointSerializer>().LifestyleSingleton());
		}
	}
}
