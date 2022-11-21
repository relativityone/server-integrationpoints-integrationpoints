using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Web.IntegrationPointsServices;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Installers.IntegrationPointsServices
{
    public static class IntegrationPointsServicesRegistration
    {
        public static IWindsorContainer AddIntegrationPointsServices(this IWindsorContainer container)
        {
            return container
                .RegisterCoreIntegrationPointsServices()
                .RegisterHelpers()
                .AddLoggingContext();
        }

        private static IWindsorContainer RegisterCoreIntegrationPointsServices(this IWindsorContainer container)
        {
            return container.Register(
                Component
                    .For<IServiceContextHelper>()
                    .ImplementedBy<ServiceContextHelperForWeb>()
                    .LifestyleTransient(),
                Component
                    .For<WebClientFactory>()
                    .UsingFactoryMethod(WebClientFactoryFactory)
                    .LifestylePerWebRequest(),
                Component
                    .For<IWorkspaceDBContext>()
                    .ImplementedBy<WorkspaceDBContext>()
                    .UsingFactoryMethod(k => new WorkspaceDBContext(k.Resolve<WebClientFactory>().CreateDbContext()))
                    .LifestyleTransient(),
                Component
                    .For<IAuthTokenGenerator>()
                    .ImplementedBy<ClaimsTokenGenerator>()
                    .LifestyleTransient(),
                Component
                    .For<ITextSanitizer>()
                    .ImplementedBy<TextSanitizer>()
                    .LifestylePerWebRequest());
        }

        private static IWindsorContainer RegisterHelpers(this IWindsorContainer container)
        {
            return container.Register(
                Component
                    .For<IFolderTreeBuilder>()
                    .ImplementedBy<FolderTreeBuilder>()
                    .LifestyleTransient());
        }

        private static WebClientFactory WebClientFactoryFactory(IKernel kernel)
        {
            IHelper helper = kernel.Resolve<IHelper>();
            IWorkspaceContext workspaceIdProvider = kernel.Resolve<IWorkspaceContext>();
            return new WebClientFactory(helper, workspaceIdProvider);
        }
    }
}
