using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;

namespace kCura.IntegrationPoints.Data.Installers
{
    public static class RepositoryRegistration
    {
        public static IWindsorContainer AddRepositories(this IWindsorContainer container)
        {
            return container.Register(
                Component
                    .For<IIntegrationPointRepository>()
                    .ImplementedBy<IntegrationPointRepository>()
                    .LifestyleTransient(),
                Component
                    .For<ISourceProviderRepository>()
                    .ImplementedBy<SourceProviderRepository>()
                    .LifestyleTransient()
                );
        }
    }
}
