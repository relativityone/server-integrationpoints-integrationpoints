using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using Relativity.API;
using System.Net.Http;
using System.Web;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using kCura.IntegrationPoints.Web.Filters;

namespace kCura.IntegrationPoints.Web.Installers
{
    public static class InfrastructureRegistration
    {
        public static IWindsorContainer AddInfrastructure(this IWindsorContainer container)
        {
            return container
                .AddCurrentHttpContext()
                .AddDelegatingHandlers()
                .AddExceptionLoggers()
                .AddSessionService()
                .AddErrorService()
                .AddExceptionFilters()
                .AddFilterProviders();
        }

        private static IWindsorContainer AddCurrentHttpContext(this IWindsorContainer container)
        {
            return container.Register(
                Component
                    .For<HttpContextBase>()
                    .UsingFactoryMethod(x => new HttpContextWrapper(HttpContext.Current))
                    .LifestylePerWebRequest(),
                Component
                    .For<HttpRequestBase>()
                    .UsingFactoryMethod(k => k.Resolve<HttpContextBase>().Request)
                    .LifestylePerWebRequest()
            );
        }

        private static IWindsorContainer AddExceptionLoggers(this IWindsorContainer container)
        {
            return container.Register(Classes
                .FromThisAssembly()
                .BasedOn<ExceptionLogger>()
                .LifestyleSingleton()
            );
        }

        private static IWindsorContainer AddDelegatingHandlers(this IWindsorContainer container)
        {
            return container.Register(Classes
                .FromThisAssembly()
                .BasedOn<DelegatingHandler>()
                .LifestyleSingleton()
            );
        }

        private static IWindsorContainer AddFilterProviders(this IWindsorContainer container)
        {
            return container.Register(Component
                .For<IFilterProvider>()
                .ImplementedBy<WindsorFilterProvider>()
                .LifestyleSingleton()
            );
        }

        private static IWindsorContainer AddErrorService(this IWindsorContainer container)
        {
            return container.Register(Component
                .For<IErrorService>()
                .ImplementedBy<CustomPageErrorService>()
                .LifestylePerWebRequest()
            );
        }

        private static IWindsorContainer AddSessionService(this IWindsorContainer container)
        {
            return container.Register(Component
                .For<ISessionService>()
                .UsingFactoryMethod(k =>
                    SessionServiceFactory.GetSessionService(
                        k.Resolve<ICPHelper>,
                        k.Resolve<IAPILog>,
                        k.Resolve<HttpContextBase>()
                    )
                )
                .LifestylePerWebRequest()
            );
        }

        private static IWindsorContainer AddExceptionFilters(this IWindsorContainer container)
        {
            return container.Register(Component
                .For<ExceptionFilter>()
                .LifestyleTransient() // we need to create single exception filter instance per each attribute instance
            );
        }
    }
}
