using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Helpers;

namespace kCura.IntegrationPoints.Data.Installers
{
    public static class HelpersRegistration
    {
        public static IWindsorContainer AddHelpers(this IWindsorContainer container)
        {
            return container.Register(
                Component
                    .For<IMassUpdateHelper>()
                    .ImplementedBy<MassUpdateHelper>()
                    .LifestyleTransient()
            );
        }
    }
}
