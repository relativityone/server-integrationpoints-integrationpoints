using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.Syncronizer;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<Services.CreateError>().ImplementedBy<Services.CreateError>());
			container.Register(Component.For<IDataProviderFactory>().AsFactory(x=>x.SelectedWith(new DataProviderComponetSelector())));
			container.Register(Component.For<IDataSyncronizerFactory>().AsFactory(x => x.SelectedWith(new DataSyncronizerComponetSelector())));
		}
	}
}
