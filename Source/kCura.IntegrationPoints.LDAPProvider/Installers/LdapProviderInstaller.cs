using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.LDAPProvider.Installers
{
    public class LdapProviderInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ILDAPSettingsReader>().ImplementedBy<LDAPSettingsReader>().LifestyleTransient());
            container.Register(Component.For<ILDAPServiceFactory>().ImplementedBy<LdapServiceFactory>().LifestyleSingleton());
            container.Register(Component.For<IDataSourceProvider>().ImplementedBy<LDAPProvider>()
                .Named(new Guid(Core.Constants.IntegrationPoints.SourceProviders.LDAP).ToString()));
        }
    }
}
