using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Services;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext
{
	internal static class WorkspaceContextRegistration
	{
		/// <summary>
		/// Registers <see cref="IWorkspaceContext"/> in a container.
		/// </summary>
		public static IWindsorContainer AddWorkspaceContext(this IWindsorContainer container)
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
				.For<IWorkspaceContext>()
				.ImplementedBy<WorkspaceContext>()
				.LifestylePerWebRequest()
			);

			return container;
		}
	}
}