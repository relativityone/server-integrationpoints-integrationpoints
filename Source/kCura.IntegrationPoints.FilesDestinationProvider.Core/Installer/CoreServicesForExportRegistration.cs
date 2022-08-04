using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
    public static class CoreServicesForExportRegistration
    {
        public static IWindsorContainer AddCoreServicesForExport(this IWindsorContainer container)
        {
            container.Register(
                Component
                    .For<IAuditManager>()
                    .ImplementedBy<CoreAuditManager>()
                    .LifestyleTransient(),
                Component
                    .For<IFieldManager>()
                    .ImplementedBy<CoreFieldManager>()
                    .LifestyleTransient(),
                Component
                    .For<WebApiServiceFactory>()
                    .LifestyleTransient(),
                Component
                    .For<CoreServiceFactory>()
                    .LifestyleTransient()
            );
            return container;
        }
    }
}
