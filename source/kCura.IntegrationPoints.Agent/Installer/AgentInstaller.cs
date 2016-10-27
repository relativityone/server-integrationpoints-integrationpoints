using System;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.kCura.IntegrationPoints.Agent;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Email;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using CreateErrorRdo = kCura.ScheduleQueue.Core.Logging.CreateErrorRdo;
using ITaskFactory = kCura.IntegrationPoints.Agent.Tasks.ITaskFactory;

namespace kCura.IntegrationPoints.Agent.Installer
{
	internal class AgentInstaller : IWindsorInstaller
	{
		private readonly IAgentHelper _agentHelper;
		private readonly Job _job;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;

		public AgentInstaller(IAgentHelper agentHelper, Job job, IScheduleRuleFactory scheduleRuleFactory)
		{
			_agentHelper = agentHelper;
			_job = job;
			_scheduleRuleFactory = scheduleRuleFactory;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			try
			{
				InstallContainer(container);
			}
			catch (Exception e)
			{
				IAPILog logger = _agentHelper.GetLoggerFactory().GetLogger().ForContext<AgentInstaller>();
				logger.LogError(e, "Unable to install container using AgentInstaller");
				throw;
			}
		}

		private void InstallContainer(IWindsorContainer container)
		{
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
			container.Kernel.AddFacility<TypedFactoryFacility>();

			container.Register(Component.For<IScheduleRuleFactory>().UsingFactoryMethod(k => _scheduleRuleFactory, true).LifestyleTransient());
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => _agentHelper, true).LifestyleTransient());
			container.Register(Component.For<IAgentHelper>().UsingFactoryMethod(k => _agentHelper, true).LifestyleTransient());
			container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForAgent>().DependsOn(Dependency.OnValue<int>(_job.WorkspaceID)).LifestyleTransient());
			container.Register(Component.For<IWorkspaceDBContext>().UsingFactoryMethod(k => new WorkspaceContext(_agentHelper.GetDBContext(_job.WorkspaceID))).LifestyleTransient());
			container.Register(Component.For<Job>().UsingFactoryMethod(k => _job).LifestyleTransient());
			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod(k => k.Resolve<RsapiClientFactory>().CreateClientForWorkspace(_job.WorkspaceID, ExecutionIdentity.System)));
			container.Register(Component.For<IRSAPIService>().Instance(new RSAPIService(container.Resolve<IHelper>(), _job.WorkspaceID)).LifestyleTransient());
			container.Register(Component.For<IRelativityConfigurationFactory>().ImplementedBy<RelativityConfigurationFactory>().LifestyleSingleton());
			container.Register(Component.For<ISendable>().ImplementedBy<SMTP>().DependsOn(Dependency.OnValue<EmailConfiguration>(container.Resolve<IRelativityConfigurationFactory>().GetConfiguration())));
			container.Register(Component.For<SyncWorker>().ImplementedBy<SyncWorker>().LifestyleTransient());
			container.Register(Component.For<SyncManager>().ImplementedBy<SyncManager>().LifestyleTransient());
			container.Register(Component.For<ExportServiceManager>().ImplementedBy<ExportServiceManager>().LifestyleTransient());
			container.Register(Component.For<SyncCustodianManagerWorker>().ImplementedBy<SyncCustodianManagerWorker>().LifestyleTransient());
			container.Register(Component.For<CreateErrorRdo>().ImplementedBy<CreateErrorRdo>().LifestyleTransient());
			container.Register(Component.For<ITaskFactory>().AsFactory(x => x.SelectedWith(new TaskComponentSelector())).LifestyleTransient());
			container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(f => f.Resolve<IHelper>().GetServicesManager()));
			container.Register(Component.For<SendEmailManager>().ImplementedBy<SendEmailManager>().LifestyleTransient());
			container.Register(Component.For<SendEmailWorker>().ImplementedBy<SendEmailWorker>().LifestyleTransient());
			container.Register(Component.For<ExportManager>().ImplementedBy<ExportManager>().LifestyleTransient());
			container.Register(Component.For<ExportWorker>().ImplementedBy<ExportWorker>().DependsOn(Dependency.OnComponent<ISynchronizerFactory, ExportDestinationSynchronizerFactory>()).LifestyleTransient());
			container.Register(Component.For<JobHistoryErrorServiceProvider>().ImplementedBy<JobHistoryErrorServiceProvider>().LifeStyle.BoundTo<ExportWorker>());
			container.Register(Component.For<IManagerFactory<ISearchManager>>().ImplementedBy<SearchManagerFactory>().LifestyleSingleton());
		}
	}
}