using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Installers
{
	public class DocumentTransferProviderInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component
				.For<IExtendedImportApiFactory>()
				.ImplementedBy<ExtendedImportApiFactory>()
				.LifestyleSingleton()
			);
			container.Register(Component
				.For<IExtendedImportApiFacade>()
				.ImplementedBy<ExtendedImportApiFacade>()
				.LifestyleTransient()
			);
			container.Register(Component
				.For<IDataSourceProvider>()
				.ImplementedBy<DocumentTransferProvider>()
				.Named(new Guid(Domain.Constants.RELATIVITY_PROVIDER_GUID).ToString())
				.LifestyleTransient()
			);
		}
	}
}
