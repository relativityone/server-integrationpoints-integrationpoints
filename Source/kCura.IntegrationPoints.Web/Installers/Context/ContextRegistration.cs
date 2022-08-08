using Castle.Windsor;
using kCura.IntegrationPoints.Web.Context.UserContext;

namespace kCura.IntegrationPoints.Web.Installers.Context
{
    public static class ContextRegistration
    {
        /// <summary>
        /// Registers <see cref="IWorkspaceContext"/> and <see cref="IUserContext"/> in a container.
        /// </summary>
        public static IWindsorContainer AddContext(this IWindsorContainer container)
        {
            return container
                .AddWorkspaceContext()
                .AddUserContext();
        }
    }
}