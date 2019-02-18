using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider.Services;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider
{
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
				.LifestylePerWebRequest()
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