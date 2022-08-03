using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Installers.Registrations
{
    internal static class SecretStoreMigrationRegistration
    {
        public static IWindsorContainer AddSecretStoreMigrator(this IWindsorContainer container)
        {
            container.Register(Component
                .For<ISecretStoreMigrator>()
                .UsingFactoryMethod(k =>
                {
                    IHelper helper = k.Resolve<IHelper>();
                    return HeartClub.SecretStoreMigratorFactory.CreateSecretStoreMigrator(helper);
                })
                .LifestyleTransient());

            container.Register(Component
                .For<ISecretStoreMigrationService>()
                .ImplementedBy<SecretStoreMigrationService>()
                .LifestyleTransient());
            return container;
        }
    }
}
