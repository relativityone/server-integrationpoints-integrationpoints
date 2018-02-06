using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.ImportProvider.Installers
{
	public class ImportProviderInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IDataSourceProvider>().ImplementedBy<ImportProvider>()
				.Named(Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE));
		}
	}
}
