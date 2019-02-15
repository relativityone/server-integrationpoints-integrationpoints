using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
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
using System.Web.Http.Controllers;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.InfrastructureServices;
using kCura.IntegrationPoints.Web.RelativityServices;
using kCura.IntegrationPoints.Web.WorkspaceIdProvider;

namespace kCura.IntegrationPoints.Web.Installers
{
	public class ControllerInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.AddFacility<TypedFactoryFacility>();

			RegisterAspNetEntryPoints(container);
			RegisterHelpers(container);

			container.Register(Component.For<WebClientFactory>().UsingFactoryMethod(kernel =>
			{
				IHelper helper = kernel.Resolve<IHelper>();
				IRsapiClientWithWorkspaceFactory rsapiClientFactory = kernel.Resolve<IRsapiClientWithWorkspaceFactory>();
				IWorkspaceIdProvider workspaceIdProvider = kernel.Resolve<IWorkspaceIdProvider>();
				return new WebClientFactory(helper, rsapiClientFactory, workspaceIdProvider);
			}).LifestylePerWebRequest());

			container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForWeb>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceDBContext>().ImplementedBy<WorkspaceContext>().UsingFactoryMethod(
				k => new WorkspaceContext(k.Resolve<WebClientFactory>().CreateDbContext())).LifestyleTransient()
			);
			container.Register(Component.For<IErrorService>().ImplementedBy<CustomPageErrorService>().LifestyleTransient());
			container.Register(Component.For<WebAPIFilterException>().ImplementedBy<WebAPIFilterException>().LifestyleSingleton());
			// TODO remove rsapi client dependency after regression tests - when it is no longer needed
			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod((k) => k.Resolve<WebClientFactory>().CreateClient()).LifestyleTransient());
			container.Register(Component.For<global::Relativity.API.IDBContext>().UsingFactoryMethod((k) => k.Resolve<WebClientFactory>().CreateDbContext()).LifestyleTransient());

			container.Register(Component.For<IRSAPIService>().UsingFactoryMethod(k => k.Resolve<IServiceContextHelper>().GetRsapiService()).LifestyleTransient());
			container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());
			container.Register(Component.For<IRsapiClientWithWorkspaceFactory>().ImplementedBy<RsapiClientWithWorkspaceFactory>().LifestyleTransient());
			
			container
				.AddLoggingContext()
				.AddRelativityServices()
				.AddServices()
				.AddWorkspaceIdProvider();
		}
		
		private static void RegisterAspNetEntryPoints(IWindsorContainer container)
		{
			container.Register(Classes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());
			container.Register(Classes.FromThisAssembly().BasedOn<IHub>().LifestyleTransient());
			container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());
		}

		private void RegisterHelpers(IWindsorContainer container)
		{
			container.Register(Component.For<IFolderTreeBuilder>().ImplementedBy<FolderTreeBuilder>().LifestyleTransient());
			container.Register(Component.For<IRelativityUrlHelper>().ImplementedBy<RelativityUrlHelper>().LifestyleTransient());
			container.Register(Component.For<SummaryPageSelector>().ImplementedBy<SummaryPageSelector>().LifestyleSingleton());
		}
	}
}