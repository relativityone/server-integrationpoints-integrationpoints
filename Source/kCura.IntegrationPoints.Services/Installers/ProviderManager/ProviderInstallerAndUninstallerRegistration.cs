using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Core.Provider.Internals;

namespace kCura.IntegrationPoints.Services.Installers.ProviderManager
{
    internal static class ProviderInstallerAndUninstallerRegistration
    {
        /// <summary>
        /// Registers <see cref="IRipProviderInstaller"/> and <see cref="IRipProviderUninstaller"/>
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IWindsorContainer AddProviderInstallerAndUninstaller(this IWindsorContainer container)
        {
            container.Register(Component
                .For<IIntegrationPointsRemover>()
                .ImplementedBy<IntegrationPointsRemover>()
                .LifestyleTransient()
            );
            container.Register(Component
                .For<IApplicationGuidFinder>()
                .ImplementedBy<ApplicationGuidFinder>()
                .LifestyleTransient()
            );
            container.Register(Component
                .For<IDataProviderFactoryFactory>()
                .ImplementedBy<DataProviderFactoryFactory>()
                .LifestyleTransient()
            );
            container.Register(Component
                .For<IRipProviderInstaller>()
                .ImplementedBy<RipProviderInstaller>()
                .LifestyleTransient()
            );
            container.Register(Component
                .For<IRipProviderUninstaller>()
                .ImplementedBy<RipProviderUninstaller>()
                .LifestyleTransient()
            );

            return container;
        }
    }
}
