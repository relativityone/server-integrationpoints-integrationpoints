using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Domain;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.CustodianManager;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Managers.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IErrorService>().ImplementedBy<Services.ErrorService>().Named("ErrorService").LifestyleTransient());
			container.Register(Component.For<Core.Services.ObjectTypeService>().ImplementedBy<Core.Services.ObjectTypeService>().LifestyleTransient());

			container.Register(Component.For<IDataProviderFactory>().ImplementedBy<AppDomainFactory>().LifestyleTransient());
			container.Register(Component.For<DomainHelper>().ImplementedBy<DomainHelper>().LifestyleTransient());

			container.Register(Component.For<ISourcePluginProvider>().ImplementedBy<DefaultSourcePluginProvider>().LifestyleTransient());

			container.Register(Component.For<IJobManager>().ImplementedBy<AgentJobManager>().LifestyleTransient());
			container.Register(Component.For<IJobService>().ImplementedBy<JobService>().LifestyleTransient());
			//container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().LifestyleTransient().DependsOn(Dependency.OnValue("agentGuid", new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)), Dependency.OnComponent<IDBContext, Dbco>()));

			var guid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);
			container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().DependsOn(Dependency.OnValue<Guid>(guid)).LifestyleTransient());

			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizerPush>().Named(typeof(RdoSynchronizerPush).AssemblyQualifiedName).LifeStyle.Transient);
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizerPull>().Named(typeof(RdoSynchronizerPull).AssemblyQualifiedName).LifeStyle.Transient);
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoCustodianSynchronizer>().Named(typeof(RdoCustodianSynchronizer).AssemblyQualifiedName).LifeStyle.Transient);

			container.Register(Component.For<RdoSynchronizerProvider>().ImplementedBy<RdoSynchronizerProvider>().LifeStyle.Transient);

			container.Register(Component.For<IRelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());
			container.Register(Component.For<IntegrationPointService>().ImplementedBy<IntegrationPointService>().LifestyleTransient());
			container.Register(
				Component.For<GetSourceProviderRdoByIdentifier>()
					.ImplementedBy<GetSourceProviderRdoByIdentifier>()
					.LifeStyle.Transient);

			container.Register(Component.For<IBatchStatus>().ImplementedBy<BatchEmail>().LifeStyle.Transient);
			container.Register(Component.For<IBatchStatus>().ImplementedBy<JobHistoryStatus>().LifeStyle.Transient);

			container.Register(Component.For<ISourceTypeFactory>().ImplementedBy<SourceTypeFactory>().LifestyleTransient());
			container.Register(Component.For<RsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifestyleTransient());

			container.Register(Component.For<RdoFilter>().ImplementedBy<RdoFilter>().LifestyleTransient());

			container.Register(Component.For<UserService>().ImplementedBy<UserService>().LifestyleTransient());
			container.Register(Component.For<ChoiceService>().ImplementedBy<ChoiceService>().LifeStyle.Transient);
			container.Register(Component.For<CustodianService>().ImplementedBy<CustodianService>().LifestyleTransient());

			container.Register(Component.For<ITabService>().ImplementedBy<RSAPITabService>().LifeStyle.Transient);
			container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<GeneralWithCustodianRdoSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());
			container.Register(Component.For<IProviderFactory>().ImplementedBy<DefaultProviderFactory>().DependsOn(new { windsorContainer = container }).LifestyleTransient());
			container.Register(Component.For<ManagerQueueService>().ImplementedBy<ManagerQueueService>().LifestyleTransient());
			container.Register(Component.For<IGuidService>().ImplementedBy<DefaultGuidService>().LifestyleTransient());
			container.Register(Component.For<JobHistoryService>().ImplementedBy<JobHistoryService>().LifestyleTransient());
			container.Register(Component.For<JobHistoryErrorService>().ImplementedBy<JobHistoryErrorService>().LifestyleTransient());

			container.Register(Component.For<IJobStatusUpdater>().ImplementedBy<JobStatusUpdater>().LifeStyle.Transient);

			container.Register(Component.For<JobTracker>().ImplementedBy<JobTracker>().LifeStyle.Transient);
			container.Register(Component.For<JobHistoryErrorQuery>().ImplementedBy<JobHistoryErrorQuery>().LifestyleTransient());
			container.Register(Component.For<TaskParameterHelper>().ImplementedBy<TaskParameterHelper>().LifestyleTransient());

			container.Register(Component.For<IImportApiFactory>().ImplementedBy<ImportApiFactory>().LifeStyle.Transient);

			container.Register(Component.For<RelativityFeaturePathService>().ImplementedBy<RelativityFeaturePathService>().LifeStyle.Transient);

			container.Register(Component.For<IExporterFactory>().ImplementedBy<ExporterFactory>().LifestyleTransient());

			if (container.Kernel.HasComponent(typeof(IHelper)))
			{
				IHelper helper = container.Resolve<IHelper>();

				// TODO: Investigate; should this be using ExecutionIdentity.CurrentUser? -- biedrzycki: April 6th, 2016
				IObjectQueryManager queryManager = helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System);
			
				container.Register(
					Component.For<IObjectQueryManagerAdaptor>()
						.ImplementedBy<ObjectQueryManagerAdaptor>()
						.DependsOn(new { objectQueryManager = queryManager})
						.LifeStyle.Transient);
				container.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>().LifestyleSingleton());
				container.Register(Component.For<IFieldRepository>().ImplementedBy<FieldRepository>().LifeStyle.Transient);
				container.Register(Component.For<IDocumentRepository>().ImplementedBy<KeplerDocumentRepository>().LifeStyle.Transient);

				// TODO: This is kind of cruddy, see if we can only use this repository through the RepositoryFactory -- biedrzycki: April 6th, 2016
				IRepositoryFactory repositoryFactory = container.Resolve<IRepositoryFactory>();
				container.Register(
					Component.For<IWorkspaceRepository>()
						.ImplementedBy<RsapiWorkspaceRepository>()
						.DependsOn(repositoryFactory.GetWorkspaceRepository())
						.LifestyleTransient());
				container.Register(Component.For<ISourceWorkspaceManager>().ImplementedBy<SourceWorkspaceManager>().LifestyleTransient());
				container.Register(Component.For<ISourceJobManager>().ImplementedBy<SourceJobManager>().LifestyleTransient());
				container.Register(Component.For<ITempDocumentTableFactory>().ImplementedBy<TempDocumentTableFactory>().LifestyleSingleton());
			}
		}
	}
}