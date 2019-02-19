using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Services;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext
{
	internal static class WorkspaceIdProviderRegistration
	{
		/// <summary>
		/// Registers <see cref="IWorkspaceIdProvider"/> in a container.
		/// </summary>
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