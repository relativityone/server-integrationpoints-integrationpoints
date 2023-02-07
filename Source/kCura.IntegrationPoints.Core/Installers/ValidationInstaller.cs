using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Core.Installers
{
    public class ValidationInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

            container.Register(Component.For<IViewErrorsPermissionValidator>().ImplementedBy<ViewErrorsPermissionValidator>().LifestyleTransient());

            container.Register(Component.For<INonValidCharactersValidator>().ImplementedBy<NonValidCharactersValidator>());

            container.Register(Component.For<IValidator>().ImplementedBy<EmailValidator>().LifestyleTransient());
            container.Register(Component.For<IValidator>().ImplementedBy<NameValidator>().LifestyleTransient());
            container.Register(Component.For<IValidator>().ImplementedBy<SchedulerValidator>().LifestyleTransient());
            container.Register(Component.For<IValidator>().ImplementedBy<IntegrationPointTypeValidator>().LifestyleTransient());
            container.Register(Component.For<IValidator>().ImplementedBy<FirstAndLastNameMappedValidator>().LifestyleTransient());

            container.Register(Component.For<IRelativityProviderValidatorsFactory>().ImplementedBy<RelativityProviderValidatorsFactory>().LifestyleTransient());
            container.Register(Component.For<IValidator>().ImplementedBy<RelativityProviderConfigurationValidator>().LifestyleTransient());

            container.Register(Component.For<IIntegrationPointProviderValidator>().ImplementedBy<IntegrationPointProviderValidator>().LifestyleTransient());

            container.Register(Component.For<IPermissionValidator>().ImplementedBy<ImportPermissionValidator>().LifestyleTransient());
            container.Register(Component.For<IPermissionValidator>().ImplementedBy<ExportPermissionValidator>().LifestyleTransient());
            container.Register(Component.For<IPermissionValidator>().ImplementedBy<PermissionValidator>().LifestyleTransient());
            container.Register(Component.For<IPermissionValidator>().ImplementedBy<SavePermissionValidator>().LifestyleTransient());
            container.Register(Component.For<IPermissionValidator>().ImplementedBy<StopJobPermissionValidator>().LifestyleTransient());
            container.Register(Component.For<IPermissionValidator>().ImplementedBy<RelativityProviderPermissionValidator>().LifestyleTransient());
            container.Register(Component.For<IPermissionValidator>().ImplementedBy<NativeCopyLinksValidator>().LifestyleTransient());

            container.Register(Component.For<IIntegrationPointPermissionValidator>().ImplementedBy<IntegrationPointPermissionValidator>().LifestyleTransient());
            container.Register(Component.For<IValidationExecutor>().ImplementedBy<ValidationExecutor>().LifestyleTransient());
        }
    }
}
