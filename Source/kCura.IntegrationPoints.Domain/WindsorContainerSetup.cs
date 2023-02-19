using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Domain.Authentication;
using Relativity.API;

namespace kCura.IntegrationPoints.Domain
{
    public class WindsorContainerSetup : IWindsorContainerSetup
    {
        private static readonly HashSet<string> _allowedInstallerAssemblies = new HashSet<string>()
        {
            "Relativity.IntegrationPoints.Contracts",
            "kCura.IntegrationPoints.Core",
            "kCura.IntegrationPoints.Data",
            "kCura.IntegrationPoints.FilesDestinationProvider.Core",
            "kCura.IntegrationPoints.FtpProvider",
            "kCura.IntegrationPoints.ImportProvider.Parser",
            "kCura.IntegrationPoints.ImportProvider",
            "kCura.IntegrationPoints.LDAPProvider",
            "kCura.IntegrationPoints.DocumentTransferProvider"
        };

        public IWindsorContainer SetUpCastleWindsor(IHelper helper)
        {
            var windsorContainer = new WindsorContainer();
            IKernel kernel = windsorContainer.Kernel;
            kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));
            windsorContainer.Register(Component.For<IHelper>().Instance(helper).LifestyleTransient());
            windsorContainer.Register(Component.For<IAuthTokenGenerator>().Instance(new ClaimsTokenGenerator()).LifestyleTransient());

            windsorContainer.Install(
                FromAssembly.InDirectory(
                    new AssemblyFilter(AppDomain.CurrentDomain.BaseDirectory)
                        .FilterByName(FilterByAllowedAssemblyNames)));

            return windsorContainer;
        }

        private bool FilterByAllowedAssemblyNames(AssemblyName assemblyName)
        {
            return _allowedInstallerAssemblies.Contains(assemblyName.Name);
        }
    }
}
