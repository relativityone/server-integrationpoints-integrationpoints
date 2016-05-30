using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
    public class ExportInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<LoggingMediatorFactory>().ImplementedBy<LoggingMediatorFactory>());
            container.Register(Component.For<ILoggingMediator>().UsingFactory((LoggingMediatorFactory f) => f.Create()));
            container.Register(
                Component.For<IUserMessageNotification, IUserNotification>().ImplementedBy<ExportUserNotification>());
            container.Register(Component.For<IExportProcessBuilder>().ImplementedBy<ExportProcessBuilder>());
            container.Register(Component.For<ExportProcessRunner>().ImplementedBy<ExportProcessRunner>());
            container.Register(Component.For<ICredentialProvider>().ImplementedBy<TokenCredentialProvider>());
        }
    }
}