using Castle.Facilities.TypedFactory;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Web.Installers
{
	public static class ContainerConfiguration
	{
		public static IWindsorContainer ConfigureContainer(this IWindsorContainer container)
		{
			return container.AddFacility<TypedFactoryFacility>();
		}
	}
}