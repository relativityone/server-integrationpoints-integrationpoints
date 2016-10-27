using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;
using Microsoft.AspNet.SignalR.Hubs;
using Relativity.API;
using Relativity.Core.Service;
using Relativity.CustomPages;
using Relativity.Toggles;
using Relativity.Toggles.Providers;

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
			container.Register(Component.For<WebClientFactory>().ImplementedBy<WebClientFactory>().LifestyleSingleton());
			container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestyleTransient());
			container.Register(Component.For<ICPHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestyleTransient());
			container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForWeb>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceDBContext>().ImplementedBy<WorkspaceContext>().UsingFactoryMethod((k) => new WorkspaceContext(k.Resolve<WebClientFactory>().CreateDbContext())).LifestyleTransient());
			container.Register(Component.For<IErrorFactory>().AsFactory().UsingFactoryMethod((k) => new ErrorFactory(container)).LifestyleSingleton());
			container.Register(Component.For<WebAPIFilterException>().ImplementedBy<WebAPIFilterException>().LifestyleSingleton());
			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod((k) => k.Resolve<WebClientFactory>().CreateClient()).LifestyleTransient());
			container.Register(Component.For<global::Relativity.API.IDBContext>().UsingFactoryMethod((k) => k.Resolve<WebClientFactory>().CreateDbContext()).LifestyleTransient());
			container.Register(Component.For<IServicesMgr>().UsingFactoryMethod((k) => k.Resolve<WebClientFactory>().CreateServicesMgr()).LifestyleTransient());
			container.Register(Component.For<GridModelFactory>().ImplementedBy<GridModelFactory>().LifestyleTransient());
			container.Register(Component.For<IRelativityUrlHelper>().ImplementedBy<RelativityUrlHelper>().LifestyleTransient());

			// TODO: we need to make use of an async GetDBContextAsync (pending Dan Wells' patch) -- biedrzycki: Feb 5th, 2016
			container.Register(Component.For<IToggleProvider>().Instance(new SqlServerToggleProvider(
				() =>
				{
					SqlConnection connection = ConnectionHelper.Helper().GetDBContext(-1).GetConnection(true);

					return connection;
				},
				async () =>
				{
					Task<SqlConnection> task = Task.Run(() =>
					{
						SqlConnection connection = ConnectionHelper.Helper().GetDBContext(-1).GetConnection(true);
						return connection;
					});

					return await task;
				})).LifestyleTransient());

			container.Register(Component.For<IHtmlSanitizerManager>().ImplementedBy<HtmlSanitizerManager>().LifestyleSingleton());
		}
	}
}