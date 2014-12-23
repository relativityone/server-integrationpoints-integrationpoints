using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IErrorService>().ImplementedBy<Services.ErrorService>().Named("ErrorService").LifestyleTransient());
			
			container.Register(Component.For<Core.Services.ObjectTypeService>().ImplementedBy<Core.Services.ObjectTypeService>());


			
			container.Register(Component.For<IDataSyncronizerFactory>().ImplementedBy<MockFactory>().DependsOn(new { container = container }));
			container.Register(Component.For<IDataProviderFactory>().ImplementedBy<MockProviderFactory>().LifestyleTransient());
			container.Register(Component.For<IJobManager>().ImplementedBy<AgentJobManager>().LifestyleTransient());
			container.Register(Component.For<IJobService>().ImplementedBy<JobService>().LifestyleTransient());
			container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().LifestyleTransient());


			container.Register(Component.For<IServiceContext>().ImplementedBy<ServiceContext>().LifestyleTransient());
			container.Register(Component.For<IntegrationPointService>().ImplementedBy<IntegrationPointService>().LifestyleTransient());

			container.Register(Component.For<SourceTypeFactory>().ImplementedBy<SourceTypeFactory>().LifestyleTransient());
			container.Register(Component.For<RsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifestyleTransient());

			container.Register(Component.For<RdoFilter>().ImplementedBy<RdoFilter>().LifestyleTransient());

			container.Register(Component.For<UserService>().ImplementedBy<UserService>().LifestyleTransient());

			container.Register(Component.For<ChoiceService>().ImplementedBy<ChoiceService>().LifeStyle.Transient);
		}
	}
}
