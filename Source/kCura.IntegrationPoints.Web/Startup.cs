using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Web.Installers;
using kCura.IntegrationPoints.Web.SignalRHubs;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Relativity.API;
using Relativity.CustomPages;

[assembly: OwinStartup(typeof(kCura.IntegrationPoints.Web.Startup))]
namespace kCura.IntegrationPoints.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
	        ICPHelper helper = ConnectionHelper.Helper();
	        IAPILog logger = helper.GetLoggerFactory().GetLogger();
			GlobalHost.HubPipeline.AddModule(new IntegrationPointDataHubErrorHandlingPipelineModule(logger));
			app.MapSignalR();
        }
    }
}