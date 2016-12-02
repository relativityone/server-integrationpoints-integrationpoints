using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
	public class ExportInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<ICredentialProvider>().ImplementedBy<TokenCredentialProvider>());

			container.Register(Component.For<LoggingMediatorFactory>().ImplementedBy<LoggingMediatorFactory>());
			container.Register(Component.For<ICompositeLoggingMediator>().UsingFactory((LoggingMediatorFactory f) => f.Create()));
			container.Register(Component.For<IUserMessageNotification, IUserNotification>().ImplementedBy<ExportUserNotification>());

			container.Register(Component.For<IDelimitersBuilder>().ImplementedBy<DelimitersBuilder>());
			container.Register(Component.For<IVolumeInfoBuilder>().ImplementedBy<VolumeInfoBuilder>());
			container.Register(Component.For<IExportFileBuilder>().ImplementedBy<ExportFileBuilder>());
			container.Register(Component.For<IExportProcessBuilder>().ImplementedBy<ExportProcessBuilder>());
			container.Register(Component.For<IExportSettingsBuilder>().ImplementedBy<ExportSettingsBuilder>());
			container.Register(Component.For<ExportProcessRunner>().ImplementedBy<ExportProcessRunner>());

			container.Register(Component.For<ICaseManagerFactory>().ImplementedBy<CaseManagerFactory>());
			container.Register(Component.For<IExporterFactory>().ImplementedBy<StoppableExporterFactory>());

			container.Register(Component.For<IConfigFactory>().ImplementedBy<ConfigFactory>().LifestyleSingleton());

			container.Register(Component.For<IServiceManagerProvider>().ImplementedBy<ServiceManagerProvider>().LifestyleTransient());

			container.Register(Component.For<IExportFieldsService>().ImplementedBy<ExportFieldsService>().LifestyleTransient());
			container.Register(Component.For<IViewService>().ImplementedBy<ViewService>().LifestyleTransient());
			container.Register(Component.For<IExportInitProcessService>().ImplementedBy<ExportInitProcessService>().LifestyleTransient());
			container.Register(Component.For<IProductionService>().ImplementedBy<ProductionService>().LifestyleTransient());
			container.Register(Component.For<IArtifactService>().ImplementedBy<ArtifactService>().LifestyleTransient());
			container.Register(Component.For<IArtifactTreeService>().ImplementedBy<ArtifactTreeService>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointValidationService>().ImplementedBy<FileDestinationProviderConfigurationValidator>().Named($"{nameof(FileDestinationProviderConfigurationValidator)}+{nameof(IIntegrationPointValidationService)}").LifestyleTransient());
		}
	}
}