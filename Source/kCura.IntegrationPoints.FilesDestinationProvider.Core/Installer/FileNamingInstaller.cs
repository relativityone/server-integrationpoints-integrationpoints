using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer
{
    public class FileNamingInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IDescriptorPartFactory>()
                .ImplementedBy<FieldDescriptorPartFactory>()
                .LifestyleSingleton());

            container.Register(Component.For<IDescriptorPartFactory>()
                .ImplementedBy<SeparatorPartDescriptorFactory>()
                .LifestyleSingleton());

            container.Register(Component.For<IDictionary<string, IDescriptorPartFactory>>()
                .UsingFactoryMethod(kernel =>
                {
                    IDescriptorPartFactory[] factories = kernel.ResolveAll<IDescriptorPartFactory>();
                    return factories.ToDictionary(factory => factory.Type, factory => factory);
                }).LifestyleSingleton().Named("DescriptorPartFactoriesDictionary"));

            container.Register(Component.For<IDescriptorPartsBuilder>()
                .ImplementedBy<DescriptorPartsBuilder>()
                .DependsOn(Dependency.OnComponent(typeof(IDictionary<string, IDescriptorPartFactory>),
                    "DescriptorPartFactoriesDictionary"))
                .LifestyleSingleton());

            container.Register(Component.For<IFileNameProvidersDictionaryBuilder>()
                .ImplementedBy<FileNameProvidersDictionaryBuilder>()
                .LifestyleSingleton());
        }
    }
}
