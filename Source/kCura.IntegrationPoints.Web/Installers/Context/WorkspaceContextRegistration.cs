using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;

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
                    .For<IWorkspaceContext>()
                    .ImplementedBy<RequestContextWorkspaceContextService>()
                    .LifestylePerWebRequest(),
                Component
                    .For<IWorkspaceContext>()
                    .ImplementedBy<SessionWorkspaceContextService>()
                    .LifestylePerWebRequest(),
                Component
                    .For<IWorkspaceContext>()
                    .ImplementedBy<NotFoundWorkspaceContextService>()
                    .LifestylePerWebRequest()
            );
        }
    }
}
