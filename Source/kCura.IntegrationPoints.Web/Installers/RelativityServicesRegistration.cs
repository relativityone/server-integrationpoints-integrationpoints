using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.IntegrationPointsServices;
using kCura.IntegrationPoints.Web.RelativityServices;
using Relativity.API;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web.Installers
{
    public static class RelativityServicesRegistration
    {
        public static IWindsorContainer AddRelativityServices(this IWindsorContainer container)
        {
            return container.Register(
                Component
                    .For<ICPHelper, IHelper>()
                    .UsingFactoryMethod(k => new RetriableCPHelperProxy(ConnectionHelper.Helper()))
                    .LifestylePerWebRequest(),
                Component
                    .For<IAPILog>()
                    .UsingFactoryMethod(k => k.Resolve<IHelper>().GetLoggerFactory().GetLogger())
                    .Named("ApiLogFromWeb")
                    .IsDefault()
                    .LifestylePerWebRequest(),
                Component
                    .For<IStringSanitizer>()
                    .UsingFactoryMethod(k => k.Resolve<IHelper>().GetStringSanitizer(Data.Constants.ADMIN_CASE_ID))
                    .LifestylePerWebRequest(),
                Component
                    .For<IRelativityObjectManagerService>()
                    .UsingFactoryMethod(k => k.Resolve<IServiceContextHelper>().GetRelativityObjectManagerService())
                    .LifestyleTransient(),
                Component
                    .For<global::Relativity.API.IDBContext>()
                    .UsingFactoryMethod(k => k.Resolve<WebClientFactory>().CreateDbContext())
                    .LifestyleTransient());
        }
    }
}
