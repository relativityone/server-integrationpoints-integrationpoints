using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
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
	public class RelativityServicesInstaler : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component
				.For<ICPHelper, IHelper>()
				.UsingFactoryMethod(k => new RetriableCPHelperProxy(ConnectionHelper.Helper()))
				.LifestyleTransient() // TODO shouldn't it be PerWebRequest?
			);
			container.Register(Component
				.For<IHtmlSanitizerManager>()
				.ImplementedBy<HtmlSanitizerManager>()
				.LifestyleSingleton()
			);
			container.Register(Component
				.For<IRSAPIService>()
				.UsingFactoryMethod(k => k.Resolve<IServiceContextHelper>().GetRsapiService())
				.LifestyleTransient()
			);
			container.Register(Component
				.For<IRSAPIClient>()
				.UsingFactoryMethod(k => k.Resolve<WebClientFactory>().CreateClient())
				.LifestyleTransient()
			);
			container.Register(Component
				.For<global::Relativity.API.IDBContext>()
				.UsingFactoryMethod(k => k.Resolve<WebClientFactory>().CreateDbContext())
				.LifestyleTransient()
			);
		}
	}
}