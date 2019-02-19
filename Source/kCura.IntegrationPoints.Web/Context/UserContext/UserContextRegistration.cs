using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
	internal static class UserContextRegistration
	{
		public static IWindsorContainer AddUserContext(this IWindsorContainer container)
		{
			container.Register(Component.For<IUserContext>().ImplementedBy<UserContext>().LifestylePerWebRequest());
			return container;
		}
	}
}