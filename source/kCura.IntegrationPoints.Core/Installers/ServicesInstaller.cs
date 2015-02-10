using System;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Domain;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IErrorService>().ImplementedBy<Services.ErrorService>().Named("ErrorService").LifestyleTransient());
			container.Register(Component.For<Core.Services.ObjectTypeService>().ImplementedBy<Core.Services.ObjectTypeService>().LifestyleTransient());

			container.Register(Component.For<IDataSyncronizerFactory>().ImplementedBy<MockFactory>().DependsOn(new { container = container }).LifeStyle.Transient);
			container.Register(Component.For<IDataProviderFactory>().ImplementedBy<AppDomainFactory>().LifestyleTransient());
			container.Register(Component.For<DomainHelper>().ImplementedBy<DomainHelper>().LifestyleTransient());

			container.Register(Component.For<ISourcePluginProvider>().ImplementedBy<DefaultSourcePluginProvider>().LifestyleTransient());

			container.Register(Component.For<IJobManager>().ImplementedBy<AgentJobManager>().LifestyleTransient());
			container.Register(Component.For<IJobService>().ImplementedBy<JobService>().LifestyleTransient());
			//container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().LifestyleTransient().DependsOn(Dependency.OnValue("agentGuid", new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)), Dependency.OnComponent<IDBContext, Dbco>()));

			var guid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);
			container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().DependsOn(Dependency.OnValue<Guid>(guid)).LifestyleTransient());

			container.Register(
				Component.For<RDOSyncronizerProvider>().ImplementedBy<RDOSyncronizerProvider>().LifeStyle.Transient);

			container.Register(
			Component.For<RDOCustodianSynchronizer>().ImplementedBy<RDOCustodianSynchronizer>().LifeStyle.Transient);

			container.Register(Component.For<RelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());
			container.Register(Component.For<IntegrationPointService>().ImplementedBy<IntegrationPointService>().LifestyleTransient());
			container.Register(
				Component.For<GetSourceProviderRdoByIdentifier>()
					.ImplementedBy<GetSourceProviderRdoByIdentifier>()
					.LifeStyle.Transient);


			container.Register(Component.For<SourceTypeFactory>().ImplementedBy<SourceTypeFactory>().LifestyleTransient());
			container.Register(Component.For<RsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifestyleTransient());

			container.Register(Component.For<RdoFilter>().ImplementedBy<RdoFilter>().LifestyleTransient());

			container.Register(Component.For<UserService>().ImplementedBy<UserService>().LifestyleTransient());
			container.Register(Component.For<ChoiceService>().ImplementedBy<ChoiceService>().LifeStyle.Transient);

			container.Register(Component.For<GeneralWithCustodianRdoSynchronizerFactory>().ImplementedBy<GeneralWithCustodianRdoSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());
		}
	}
}
