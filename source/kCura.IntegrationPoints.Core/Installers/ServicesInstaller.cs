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

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IErrorService>().ImplementedBy<Services.ErrorService>().Named("ErrorService").LifestyleTransient());
			container.Register(Component.For<IDataProviderFactory>().AsFactory(x => x.SelectedWith(new DataProviderComponetSelector())).LifestyleTransient());
			container.Register(Component.For<IDataSyncronizerFactory>().AsFactory(x => x.SelectedWith(new DataSyncronizerComponetSelector())).LifestyleTransient());
			container.Register(Component.For<IServiceContext>().ImplementedBy<ServiceContext>().LifestyleTransient());
			container.Register(Component.For<IntegrationPointReader>().ImplementedBy<IntegrationPointReader>().LifestyleTransient());

			container.Register(Component.For<SourceTypeFactory>().ImplementedBy<SourceTypeFactory>().LifestyleTransient());
			container.Register(Component.For<RdoFilter>().ImplementedBy<RdoFilter>().LifestyleTransient());

			container.Register(Component.For<UserService>().ImplementedBy<UserService>().LifestyleTransient());
		}
	}
}
