using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
    public static class ExportRepositoryRegistration
    {
        public static IWindsorContainer AddExportRepositories(this IWindsorContainer container)
        {
            container.Register(
                Component.For<IFileRepository>()
                    .ImplementedBy<FileRepository>()
                    .LifestyleTransient()
            );

            return container;
        }
    }
}
