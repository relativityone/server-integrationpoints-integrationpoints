using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Logging;
using kCura.Relativity.Client;
using Microsoft.AspNet.SignalR.Hubs;
using Relativity.API;
using Relativity.Core.Service;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web.Installers
{
	public class ControllerInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.AddFacility<TypedFactoryFacility>();

			container.Register(Classes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());
			container.Register(Classes.FromThisAssembly().BasedOn<IHub>().LifestyleTransient());
			container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());

			container.Register(Component.For<IWorkspaceService>().ImplementedBy<ControllerCustomPageService>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceService>().ImplementedBy<WebAPICustomPageService>().LifestyleTransient());
			container.Register(Component.For<IConfig>().Instance(Config.Config.Instance).LifestyleSingleton());
			container.Register(Component.For<ISessionService>().UsingFactoryMethod(k => SessionService.Session).LifestylePerWebRequest());
			container.Register(Component.For<WebClientFactory>().UsingFactoryMethod(kernel =>
			{
				var rsapiClientFactory = kernel.Resolve<IRsapiClientFactory>();
				IEnumerable<IWorkspaceService> services = kernel.ResolveAll<IWorkspaceService>();
				return new WebClientFactory(ConnectionHelper.Helper(), rsapiClientFactory, services);
			}).LifestyleSingleton());
			container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestyleTransient());
			container.Register(Component.For<ICPHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestyleTransient());
			container.Register(Component.For<IFolderTreeBuilder>().ImplementedBy<FolderTreeBuilder>().LifestyleTransient());
			container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForWeb>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceDBContext>().ImplementedBy<WorkspaceContext>().UsingFactoryMethod((k) => new WorkspaceContext(k.Resolve<WebClientFactory>().CreateDbContext())).LifestyleTransient());
			container.Register(Component.For<IErrorService>().ImplementedBy<CustomPageErrorService>().LifestyleTransient());
			container.Register(Component.For<WebAPIFilterException>().ImplementedBy<WebAPIFilterException>().LifestyleSingleton());
			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod((k) => k.Resolve<WebClientFactory>().CreateClient()).LifestyleTransient());
			container.Register(Component.For<global::Relativity.API.IDBContext>().UsingFactoryMethod((k) => k.Resolve<WebClientFactory>().CreateDbContext()).LifestyleTransient());
			container.Register(Component.For<GridModelFactory>().ImplementedBy<GridModelFactory>().LifestyleTransient());
			container.Register(Component.For<IRelativityUrlHelper>().ImplementedBy<RelativityUrlHelper>().LifestyleTransient());
			container.Register(Component.For<IRSAPIService>().UsingFactoryMethod(k => k.Resolve<IServiceContextHelper>().GetRsapiService()).LifestyleTransient());
			container.Register(Component.For<IHtmlSanitizerManager>().ImplementedBy<HtmlSanitizerManager>().LifestyleSingleton());
			container.Register(Component.For<SummaryPageSelector>().ImplementedBy<SummaryPageSelector>().LifestyleSingleton());
			container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());
			container.Register(Component.For<IRsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifestyleTransient());
			container.Register(Component.For<ICacheHolder>().ImplementedBy<CacheHolder>().LifestyleSingleton());
			container.Register(Component.For<IWebCorrelationContextProvider>().ImplementedBy<WebActionContextProvider>().LifestyleTransient());
		}
	}
}