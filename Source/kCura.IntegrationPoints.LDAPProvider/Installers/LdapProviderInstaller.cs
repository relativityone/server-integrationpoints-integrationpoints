using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Security;

namespace kCura.IntegrationPoints.LDAPProvider.Installers
{
    public class LdapProviderInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ILDAPSettingsReader>().ImplementedBy<LDAPSettingsReader>().LifestyleTransient());
            container.Register(Component.For<ILDAPServiceFactory>().ImplementedBy<LdapServiceFactory>().LifestyleSingleton());
        }
    }
}
