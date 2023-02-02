using System;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Web.Extensions;
using kCura.IntegrationPoints.Web.Infrastructure.ExceptionLoggers;
using kCura.IntegrationPoints.Web.Infrastructure.MessageHandlers;
using Newtonsoft.Json;
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

            Apps.Common.Config.Manager.Settings.Factory =
                new HelperConfigSqlServiceFactoryWrapper(ConnectionHelper.Helper);
            CreateWindsorContainer();

            WebApiConfig.Register(GlobalConfiguration.Configuration);

            GlobalConfiguration.Configuration.AddMessageHandler(
                handler: _container.Resolve<CorrelationIdHandler>()
            );
            GlobalConfiguration.Configuration.AddWebAPIFiltersProvider(
                filterProvider: _container.Resolve<System.Web.Http.Filters.IFilterProvider>()
            );
            GlobalConfiguration.Configuration.AddExceptionLogger(
                exceptionLogger: _container.Resolve<WebAPIExceptionLogger>()
            );

            FilterConfig.RegisterGlobalMvcFilters(GlobalFilters.Filters);
            FilterConfig.RegisterGlobalApiFilters(GlobalConfiguration.Configuration.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            MediaTypeFormatterCollection formatters = GlobalConfiguration.Configuration.Formatters;
            JsonMediaTypeFormatter jsonFormatter = formatters.JsonFormatter;
            JsonSerializerSettings settings = jsonFormatter.SerializerSettings;
            settings.SetupDefaults();
            settings.Formatting = Formatting.Indented;
            GlobalConfiguration.Configuration.EnsureInitialized();
        }

        public void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();

            ICPHelper helper = ConnectionHelper.Helper();
            IAPILog log = helper.GetLoggerFactory().GetLogger();

            //log.LogError(exception, "Exception occurred in Integration Points Custom Page.");

            var errorRdoCreator = new CreateErrorRdoQuery(helper.GetServicesManager(), log);
            var errorService = new CustomPageErrorService(errorRdoCreator, log);
            var errorModel = new ErrorModel(exception)
            {
                Location = "Global error handler",
            };
            errorService.Log(errorModel);
        }

        protected void Application_End()
        {
            _container?.Dispose();
        }

        private void CreateWindsorContainer()
        {
            _container = new WindsorContainer();
            IKernel kernel = _container.Kernel;
            kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));

            _container.Install(
                FromAssembly.InDirectory(new AssemblyFilter(HttpRuntime.BinDirectory,
                    "kCura.IntegrationPoints*.dll"))); //<--- DO NOT CHANGE THIS LINE

            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(_container.Kernel));
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator),
                new WindsorCompositionRoot(_container, ConnectionHelper.Helper().GetLoggerFactory().GetLogger()));
            WindsorServiceLocator.Container = _container;
        }
    }
}