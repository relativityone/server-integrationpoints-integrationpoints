using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Web.Installers
{
    public static class ContainerConfiguration
    {
        public static IWindsorContainer ConfigureContainer(this IWindsorContainer container)
        {
            container.Register(Component
                .For<ILazyComponentLoader>()
                .ImplementedBy<LazyOfTComponentLoader>()
            );

            return container.AddFacility<TypedFactoryFacility>();
        }
    }
}