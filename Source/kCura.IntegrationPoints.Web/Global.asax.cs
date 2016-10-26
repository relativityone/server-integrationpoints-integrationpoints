using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer;
using kCura.IntegrationPoints.Web.Installers;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Relativity.API;
using Relativity.CustomPages;
using FtpProviderInstaller = kCura.IntegrationPoints.FtpProvider.Installers.ServicesInstaller;
using CoreInstaller = kCura.IntegrationPoints.Core.Installers.ServicesInstaller;
using KeywordInstaller = kCura.IntegrationPoints.Core.Installers.KeywordInstaller;

namespace kCura.IntegrationPoints.Web
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static IWindsorContainer _container;

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			kCura.Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(ConnectionHelper.Helper());
			CreateWindsorContainer();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
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
			var error = Server.GetLastError();
			new CreateErrorRdo(ConnectionHelper.Helper().GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System)).Execute(0,"Application", error);
		}

		private void CreateWindsorContainer()
		{
			_container = new WindsorContainer();
			var kernel = _container.Kernel;
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));
			_container.Install(new CoreInstaller());
			_container.Install(new FtpProviderInstaller());
			_container.Install(new KeywordInstaller());
			_container.Install(new QueryInstallers());
			_container.Install(new ExportInstaller());
			_container.Install(new ControllerInstaller());

			ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(_container.Kernel));
			GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new WindsorCompositionRoot(_container));
		}

		protected void Application_End()
		{
			_container.Dispose();
		}

	}
}