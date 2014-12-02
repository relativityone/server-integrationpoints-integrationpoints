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
using kCura.IntegrationPoints.Core.Services.Syncronizer;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IErrorService>().ImplementedBy<Services.ErrorService>().Named("ErrorService"));
			container.Register(Component.For<IDataProviderFactory>().AsFactory(x=>x.SelectedWith(new DataProviderComponetSelector())));
			container.Register(Component.For<IDataSyncronizerFactory>().AsFactory(x => x.SelectedWith(new DataSyncronizerComponetSelector())));
			container.Register(Component.For<IServiceContext>().ImplementedBy<ServiceContext>());
			container.Register(Component.For<IntegrationPointHelper>().ImplementedBy<IntegrationPointHelper>());

			container.Register(Component.For<IntegrationPointReader>().ImplementedBy<IntegrationPointReader>());
			container.Register(Component.For<SourceTypeFactory>().ImplementedBy<SourceTypeFactory>());
			container.Register(Component.For<RdoFilter>().ImplementedBy<RdoFilter>());
		}
	}
}
