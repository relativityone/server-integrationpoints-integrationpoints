using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Services;

namespace kCura.IntegrationPoints.Web.Installers.Context
{
	internal static class WorkspaceContextRegistration
	{
		/// <summary>
		/// Registers <see cref="IWorkspaceContext"/> in a container.
		/// </summary>
		public static IWindsorContainer AddWorkspaceContext(this IWindsorContainer container)
		{
			return container.Register(
				Component
					.For<IWorkspaceService>()
					.ImplementedBy<WebApiCustomPageService>()
					.LifestylePerWebRequest(),
				Component
					.For<IWorkspaceService>()
					.ImplementedBy<ControllerCustomPageService>()
					.LifestylePerWebRequest(),
				Component
					.For<IWorkspaceContext>()
					.ImplementedBy<WorkspaceContext>()
					.LifestylePerWebRequest()
			);
		}
	}
}