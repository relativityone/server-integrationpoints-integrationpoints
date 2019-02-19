using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Context.UserContext.Services;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
	internal static class UserContextRegistration
	{
		/// <summary>
		/// Registers <see cref="IUserContext"/> in container
		/// </summary>
		public static IWindsorContainer AddUserContext(this IWindsorContainer container)
		{
			container.Register(Component
				.For<IUserContextService>()
				.ImplementedBy<RequestHeadersUserContextService>()
				.LifestyleSingleton()
			);
			container.Register(Component
				.For<IUserContextService>()
				.ImplementedBy<SessionUserContextService>()
				.LifestylePerWebRequest()
			);
			container.Register(Component
				.For<IUserContext>()
				.ImplementedBy<UserContext>()
				.LifestylePerWebRequest()
			);
			return container;
		}
	}
}