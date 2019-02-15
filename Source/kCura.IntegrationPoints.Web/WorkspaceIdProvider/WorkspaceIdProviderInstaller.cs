using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Web.WorkspaceIdProvider.Services;

namespace kCura.IntegrationPoints.Web.WorkspaceIdProvider
{
	// TODO it assumes that IAPILog and ICPHelper are already registered
	internal static class WorkspaceIdProviderInstaller
	{
		/// <summary>
		/// Registers IWorkspaceIdProvider in a container.
		/// </summary>
		/// <param name="container"></param>
		/// <returns></returns>
		public static IWindsorContainer AddWorkspaceIdProvider(this IWindsorContainer container)
		{
			container.Register(Component
				.For<IWorkspaceService>()
				.ImplementedBy<WebApiCustomPageService>()
				.LifestyleSingleton()
			);
			container.Register(Component
				.For<IWorkspaceService>()
				.ImplementedBy<ControllerCustomPageService>()
				.LifestyleTransient()
			);
			container.Register(Component
				.For<IWorkspaceIdProvider>()
				.ImplementedBy<WorkspaceIdProvider>()
				.LifestylePerWebRequest()
			);

			return container;
		}
	}
}