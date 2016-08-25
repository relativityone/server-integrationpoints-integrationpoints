using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
	[Obsolete("This class is obsolete as it does not conform to our usage of the Composition Root.")]
	public class ExportInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<ICredentialProvider>().ImplementedBy<TokenCredentialProvider>());

			container.Register(Component.For<LoggingMediatorFactory>().ImplementedBy<LoggingMediatorFactory>());
			container.Register(Component.For<ILoggingMediator>().UsingFactory((LoggingMediatorFactory f) => f.Create()));
			container.Register(Component.For<IUserMessageNotification, IUserNotification>().ImplementedBy<ExportUserNotification>());

			container.Register(Component.For<IDelimitersBuilder>().ImplementedBy<DelimitersBuilder>());
			container.Register(Component.For<IVolumeInfoBuilder>().ImplementedBy<VolumeInfoBuilder>());
			container.Register(Component.For<IExportFileBuilder>().ImplementedBy<ExportFileBuilder>());
			container.Register(Component.For<IExportProcessBuilder>().ImplementedBy<ExportProcessBuilder>());
			container.Register(Component.For<IExportSettingsBuilder>().ImplementedBy<ExportSettingsBuilder>());
			container.Register(Component.For<ExportProcessRunner>().ImplementedBy<ExportProcessRunner>());

			container.Register(Component.For<ICaseManagerFactory>().ImplementedBy<CaseManagerWrapperFactory>());
			container.Register(Component.For<IExporterFactory>().ImplementedBy<StoppableExporterFactory>());
			container.Register(Component.For<ISearchManagerFactory>().ImplementedBy<SearchManagerFactory>());

			container.Register(Component.For<IConfigFactory>().ImplementedBy<ConfigFactory>());
		}
	}
}