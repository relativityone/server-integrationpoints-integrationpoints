using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using Relativity.DataExchange.Io;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
	public class ExportInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<LoggingMediatorFactory>().ImplementedBy<LoggingMediatorFactory>().LifestyleTransient());
			container.Register(Component.For<ICompositeLoggingMediator>().UsingFactory((LoggingMediatorFactory f) => f.Create()).LifestyleTransient());
			container.Register(Component.For<IUserMessageNotification, IUserNotification>().ImplementedBy<ExportUserNotification>());

			container.Register(Component.For<IDelimitersBuilder>().ImplementedBy<DelimitersBuilder>());
			container.Register(Component.For<IVolumeInfoBuilder>().ImplementedBy<VolumeInfoBuilder>());
			container.Register(Component.For<IExportedObjectBuilder>().ImplementedBy<ExportedObjectBuilder>());
			container.Register(Component.For<IExportedArtifactNameRepository>().ImplementedBy<ExportedArtifactNameRepository>());
			container.Register(Component.For<IExportFileBuilder>().ImplementedBy<ExportFileBuilder>());
			container.Register(Component.For<IExportProcessBuilder>().ImplementedBy<ExportProcessBuilder>().LifestyleTransient());
			container.Register(Component.For<IExportSettingsBuilder>().ImplementedBy<ExportSettingsBuilder>());
			container.Register(Component.For<ExportProcessRunner>().ImplementedBy<ExportProcessRunner>().LifestyleTransient());

			container.Register(Component.For<ICaseManagerFactory>().ImplementedBy<CaseManagerFactory>());

			container.Register(Component.For<IFactoryConfigBuilder>().ImplementedBy<FactoryConfigBuilder>().LifestyleTransient());
			container.Register(Component.For<IExtendedExporterFactory>().ImplementedBy<ExtendedExporterFactory>().LifestyleTransient());

			container.Register(Component.For<IExportFieldsService>().ImplementedBy<ExportFieldsServiceProxy>().LifestyleTransient());
			container.Register(Component.For<IViewService>().ImplementedBy<ViewServiceProxy>().LifestyleTransient());

			container.Register(Component.For<IExportInitProcessService>().ImplementedBy<ExportInitProcessService>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointValidationService>().ImplementedBy<FileDestinationProviderConfigurationValidator>().Named($"{nameof(FileDestinationProviderConfigurationValidator)}+{nameof(IIntegrationPointValidationService)}").LifestyleTransient());

			container.Register(Component.For<IJobInfoFactory>().ImplementedBy<JobInfoFactory>().LifestyleTransient());
			container.Register(Component.For<IDirectory>().ImplementedBy<Helpers.LongPathDirectoryHelper>().LifestyleTransient());
			container.Register(Component.For<IExportConfig>().ImplementedBy<LoadFileExportConfig>().LifestyleTransient());
			container.Register(Component.For<IExportServiceFactory>().ImplementedBy<ExportServiceFactory>().LifestyleTransient());

			container.AddExportRepositories();
			container.AddCoreServicesForExport();
		}
	}
}