using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Domain;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.DestinationTypes;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.CustodianManager;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;
using Relativity.Toggles;
using Relativity.Toggles.Providers;

namespace kCura.IntegrationPoints.Core.Installers
{
    public class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (container.Kernel.HasComponent(typeof(ISerializer)) == false)
            {
                container.Register(Component.For<ISerializer>().ImplementedBy<JSONSerializer>().LifestyleTransient());
            }

            container.Register(Component.For<IObjectTypeRepository>().ImplementedBy<RsapiObjectTypeRepository>().UsingFactoryMethod(x =>
            {
                IServiceContextHelper contextHelper = x.Resolve<IServiceContextHelper>();
                IHelper helper = x.Resolve<IHelper>();
                return new RsapiObjectTypeRepository(helper, contextHelper.WorkspaceID);
            }));

            container.Register(Component.For<IContextContainerFactory>().ImplementedBy<ContextContainerFactory>().LifestyleSingleton());

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
			container.Register(Component.For<IDataSynchronizer>().ImplementedBy<ExportSynchroznizer>().Named(typeof(ExportSynchroznizer).AssemblyQualifiedName).LifeStyle.Transient);

            container.Register(Component.For<IRdoSynchronizerProvider>().ImplementedBy<RdoSynchronizerProvider>().LifeStyle.Transient);

            container.Register(Component.For<IRelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());
            container.Register(Component.For<IIntegrationPointService>().ImplementedBy<IntegrationPointService>().LifestyleTransient());
            container.Register(
                Component.For<GetSourceProviderRdoByIdentifier>()
                    .ImplementedBy<GetSourceProviderRdoByIdentifier>()
                    .LifeStyle.Transient);

            container.Register(Component.For<IBatchStatus>().ImplementedBy<BatchEmail>().LifeStyle.Transient);
			container.Register(Component.For<IBatchStatus>().ImplementedBy<JobHistoryBatchUpdateStatus>().LifeStyle.Transient);

            container.Register(Component.For<ISourceTypeFactory>().ImplementedBy<SourceTypeFactory>().LifestyleTransient());
			container.Register(Component.For<IDestinationTypeFactory>().ImplementedBy<DestinationTypeFactory>().LifestyleTransient());
            container.Register(Component.For<RsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifestyleTransient());

            container.Register(Component.For<RdoFilter>().ImplementedBy<RdoFilter>().LifestyleTransient());

            container.Register(Component.For<UserService>().ImplementedBy<UserService>().LifestyleTransient());
            container.Register(Component.For<ChoiceService>().ImplementedBy<ChoiceService>().LifeStyle.Transient);
            container.Register(Component.For<CustodianService>().ImplementedBy<CustodianService>().LifestyleTransient());

            container.Register(Component.For<ITabService>().ImplementedBy<RSAPITabService>().LifeStyle.Transient);
            container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<GeneralWithCustodianRdoSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());
			container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<ExportDestinationSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());

            container.Register(Component.For<IProviderFactory>().ImplementedBy<DefaultProviderFactory>().DependsOn(new { windsorContainer = container }).LifestyleTransient());
            container.Register(Component.For<ManagerQueueService>().ImplementedBy<ManagerQueueService>().LifestyleTransient());
            container.Register(Component.For<IGuidService>().ImplementedBy<DefaultGuidService>().LifestyleTransient());
            container.Register(Component.For<IJobHistoryService>().ImplementedBy<JobHistoryService>().LifestyleTransient());

			container.Register(Component.For<JobHistoryErrorService>().ImplementedBy<JobHistoryErrorService>().LifestyleTransient());

	        container.Register(Component.For<IJobHistoryErrorService>()
		        .UsingFactoryMethod((k) => k.Resolve<JobHistoryErrorService>()).LifestyleTransient());

			container.Register(Component.For<IJobStatusUpdater>().ImplementedBy<JobStatusUpdater>().LifeStyle.Transient);

            container.Register(Component.For<JobTracker>().ImplementedBy<JobTracker>().LifeStyle.Transient);
            container.Register(Component.For<JobHistoryErrorQuery>().ImplementedBy<JobHistoryErrorQuery>().LifestyleTransient());
            container.Register(Component.For<TaskParameterHelper>().ImplementedBy<TaskParameterHelper>().LifestyleTransient());

            container.Register(Component.For<IImportApiFactory>().ImplementedBy<ImportApiFactory>().LifeStyle.Transient);
            container.Register(Component.For<ISystemEventLoggingService>().ImplementedBy<SystemEventLoggingService>().LifeStyle.Transient);

            container.Register(Component.For<RelativityFeaturePathService>().ImplementedBy<RelativityFeaturePathService>().LifeStyle.Transient);

            container.Register(Component.For<IExporterFactory>().ImplementedBy<ExporterFactory>().LifestyleTransient());

			container.Register(Component.For<IManagerFactory>().ImplementedBy<ManagerFactory>().LifestyleTransient());

            container.Register(Component.For<IEncryptionManager>().ImplementedBy<DefaultEncryptionManager>().LifestyleSingleton());

            if (container.Kernel.HasComponent(typeof(IHelper)))
            {
                container.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>().LifestyleSingleton());

                // TODO: This is kind of cruddy, see if we can only use this repository through the RepositoryFactory -- biedrzycki: April 6th, 2016
                container.Register(
                    Component.For<IWorkspaceRepository>()
                        .ImplementedBy<KeplerWorkspaceRepository>()
                        .UsingFactoryMethod((k) => k.Resolve<IRepositoryFactory>().GetWorkspaceRepository())
                        .LifestyleTransient());
                container.Register(Component.For<ISourceWorkspaceManager>().ImplementedBy<SourceWorkspaceManager>().LifestyleTransient());
                container.Register(Component.For<ISourceJobManager>().ImplementedBy<SourceJobManager>().LifestyleTransient());
            }
        }
    }
}