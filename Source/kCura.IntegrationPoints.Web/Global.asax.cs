using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Web.MessageHandlers;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Relativity.API;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : HttpApplication
	{
		private IWindsorContainer _container;

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(ConnectionHelper.Helper());
			CreateWindsorContainer();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			WebApiConfig.AddMessageHandlers(GlobalConfiguration.Configuration, ConnectionHelper.Helper());
			FilterConfig.RegisterWebAPIFilters(GlobalConfiguration.Configuration, _container);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			var formatters = GlobalConfiguration.Configuration.Formatters;
			var jsonFormatter = formatters.JsonFormatter;
			var settings = jsonFormatter.SerializerSettings;
			settings.Formatting = Formatting.Indented;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			GlobalConfiguration.Configuration.EnsureInitialized();
		}

		public void Application_Error(object sender, EventArgs e)
		{
			Exception exception = Server.GetLastError();
			var rsapiClientFactory = new RsapiClientFactory(ConnectionHelper.Helper());
			var errorRdoCreator = new CreateErrorRdoQuery(rsapiClientFactory);
			var errorService = new CustomPageErrorService(errorRdoCreator);
			var errorModel = new ErrorModel(exception)
			{
				Location = "Global error handler",
			};
			errorService.Log(errorModel);
		}

		private void CreateWindsorContainer()
		{
			_container = new WindsorContainer();
			var kernel = _container.Kernel;
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));

			_container.Install(FromAssembly.InDirectory(new AssemblyFilter(HttpRuntime.BinDirectory, "kCura.IntegrationPoints*.dll"))); //<--- DO NOT CHANGE THIS LINE

			ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(_container.Kernel));
			GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new WindsorCompositionRoot(_container));
		}

		protected void Application_End()
		{
			_container.Dispose();
		}

	}
}