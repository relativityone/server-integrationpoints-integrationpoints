using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.ImportProvider.Installers
{
	public class ImportProviderInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			if (!container.Kernel.HasComponent(typeof(Func<ISearchManager>)))
			{
				container.Register(Component.For<Func<ISearchManager>>()
					.UsingFactoryMethod(k => (Func<ISearchManager>)(() => k.Resolve<IServiceManagerProvider>().Create<ISearchManager, SearchManagerFactory>()))
					.LifestyleTransient()
				);
			}

			container.Register(Component.For<IDataSourceProvider>().ImplementedBy<ImportProvider>()
				.Named(Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE));
		}
	}
}
