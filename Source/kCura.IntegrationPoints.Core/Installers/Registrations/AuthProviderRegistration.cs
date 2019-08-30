using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Authentication.AuthProvider;

namespace kCura.IntegrationPoints.Core.Installers.Registrations
{
	internal static class AuthProviderRegistration
	{
		public static IWindsorContainer AddAuthProvider(this IWindsorContainer container) // TODO unit tests
		{
			container.Register(Component
				.For<IAuthProvider>()
				.ImplementedBy<AuthProviderRetryDecorator>()
				.LifestyleTransient()
			);
			container.Register(Component
				.For<IAuthProvider>()
				.ImplementedBy<AuthProviderInstrumentationDecorator>()
				.LifestyleTransient()
			);
			container.Register(Component
				.For<IAuthProvider>()
				.ImplementedBy<AuthProvider>()
				.LifestyleSingleton()
			);
			return container;
		}
	}
}
