using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using Relativity.API;

namespace Relativity.IntegrationPoints.Services.Installers.Context
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
                    .For<IRelativityObjectManagerService>()
                    .UsingFactoryMethod(
                        k => new RelativityObjectManagerService(k.Resolve<IHelper>(), workspaceID),
                        managedExternally: true),
                Component
                    .For<IServiceContextHelper>()
                    .UsingFactoryMethod(
                        k => new ServiceContextHelperForKeplerService(
                            k.Resolve<IServiceHelper>(), workspaceID)),
                Component
                    .For<IWorkspaceDBContext>()
                    .ImplementedBy<WorkspaceDBContext>()
                    .UsingFactoryMethod(
                        k => new WorkspaceDBContext(
                            k.Resolve<IHelper>().GetDBContext(workspaceID)))
                    .LifestyleTransient());

            return container;
        }
    }
}
