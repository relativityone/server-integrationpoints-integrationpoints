using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Facades.SecretStore;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;

namespace kCura.IntegrationPoints.Data.Installers
{
    public static class RepositoryRegistration
    {
        public static IWindsorContainer AddRepositories(this IWindsorContainer container)
        {
            container.RegisterSecretsRepository();

            return container.Register(
                Component
                    .For<IIntegrationPointRepository>()
                    .ImplementedBy<IntegrationPointRepository>()
                    .LifestyleTransient(),
                Component
                    .For<ISourceProviderRepository>()
                    .ImplementedBy<SourceProviderRepository>()
                    .LifestyleTransient(),
                Component
                    .For<IDestinationProviderRepository>()
                    .ImplementedBy<DestinationProviderRepository>()
                    .LifestyleTransient(),
                Component
                    .For<IDocumentRepository>()
                    .ImplementedBy<KeplerDocumentRepository>()
                    .LifestyleTransient(),
                Component
                    .For<IChoiceRepository>()
                    .ImplementedBy<CachedChoiceRepository>()
                    .LifestyleTransient(),
                Component
                    .For<IChoiceRepository>()
                    .ImplementedBy<ChoiceRepository>()
                    .LifestyleTransient(),
                Component
                    .For<IRepositoryFactory>()
                    .ImplementedBy<RepositoryFactory>()
                    .LifestyleTransient(),
                Component
                    .For<IWorkspaceRepository>()
                    .ImplementedBy<KeplerWorkspaceRepository>()
                    .UsingFactoryMethod(k => k.Resolve<IRepositoryFactory>().GetWorkspaceRepository())
                    .LifestyleTransient()
            );
        }

        private static void RegisterSecretsRepository(this IWindsorContainer container)
        {
            container.Register(
                Component.For<ISecretStoreFacade>()
                    .ImplementedBy<SecretStoreFacadeRetryDecorator>()
                    .LifestyleTransient(),
                Component.For<ISecretStoreFacade>()
                    .ImplementedBy<SecretStoreFacadeInstrumentationDecorator>()
                    .LifestyleTransient(),
                Component.For<ISecretStoreFacade>()
                    .ImplementedBy<SecretStoreFacade>()
                    .LifestyleTransient(),
                Component.For<ISecretsRepository>()
                    .ImplementedBy<SecretsRepository>()
                    .LifestyleTransient()
            );
        }
    }
}
