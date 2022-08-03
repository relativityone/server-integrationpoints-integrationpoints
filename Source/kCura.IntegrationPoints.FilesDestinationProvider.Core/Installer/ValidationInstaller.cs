using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
    public class ValidationInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IFileDestinationProviderValidatorsFactory>()
                .ImplementedBy<FileDestinationProviderValidatorsFactory>()
                .LifestyleTransient()
            );

            // only register provider's top most validator
            container.Register(Component.For<IValidator>()
                .ImplementedBy<FileDestinationProviderConfigurationValidator>()
                .Named($"{nameof(FileDestinationProviderConfigurationValidator)}+{nameof(IValidator)}")
                .LifestyleTransient()
            );

            container.Register(Component.For<IPermissionValidator>().ImplementedBy<PermissionValidator>().LifestyleTransient());
        }
    }
}