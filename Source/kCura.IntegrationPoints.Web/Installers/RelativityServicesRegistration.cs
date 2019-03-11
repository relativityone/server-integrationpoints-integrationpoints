using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.IntegrationPointsServices;
using kCura.IntegrationPoints.Web.RelativityServices;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core.Service;
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
					.For<IHtmlSanitizerManager>()
					.ImplementedBy<HtmlSanitizerManager>()
					.LifestyleSingleton(),
				Component
					.For<IRSAPIService>()
					.UsingFactoryMethod(k => k.Resolve<IServiceContextHelper>().GetRsapiService())
					.LifestyleTransient(),
				Component
					.For<IRSAPIClient>()
					.UsingFactoryMethod(k => k.Resolve<WebClientFactory>().CreateClient())
					.LifestyleTransient(),
				Component
					.For<global::Relativity.API.IDBContext>()
					.UsingFactoryMethod(k => k.Resolve<WebClientFactory>().CreateDbContext())
					.LifestyleTransient()
			);
		}
	}
}