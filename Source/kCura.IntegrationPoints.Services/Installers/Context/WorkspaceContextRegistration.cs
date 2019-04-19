using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Installers.Context
{
    internal static class WorkspaceContextRegistration
    {
        /// <summary>
        /// Registers workspace specific services
        /// </summary>
        /// <param name="container"></param>
        /// <param name="workspaceID"></param>
        /// <returns></returns>
        public static IWindsorContainer AddWorkspaceContext(this IWindsorContainer container, int workspaceID)
        {
            container.Register(
                Component
                    .For<IRSAPIService>()
                    .UsingFactoryMethod(
                        k => new RSAPIService(k.Resolve<IHelper>(), workspaceID),
                        managedExternally: true),
                Component
                    .For<IServiceContextHelper>()
                    .UsingFactoryMethod(k => new ServiceContextHelperForKeplerService(k.Resolve<IServiceHelper>(), workspaceID)
                ),
                Component
                    .For<IWorkspaceDBContext>()
                    .ImplementedBy<WorkspaceContext>()
                    .UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(workspaceID)))
                    .LifestyleTransient()
            );

            return container;
        }
    }
}
