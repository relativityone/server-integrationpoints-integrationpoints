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
			return container.Register(
				Component
					.For<IUserContextService>()
					.ImplementedBy<RequestHeadersUserContextService>()
					.LifestylePerWebRequest(),
				Component
					.For<IUserContextService>()
					.ImplementedBy<SessionUserContextService>()
					.LifestylePerWebRequest(),
				Component
					.For<IUserContext>()
					.ImplementedBy<UserContext>()
					.LifestylePerWebRequest()
			);
		}
	}
}