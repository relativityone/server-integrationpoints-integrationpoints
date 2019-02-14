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

[assembly: OwinStartup(typeof(kCura.IntegrationPoints.Web.Startup))]
namespace kCura.IntegrationPoints.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
	        GlobalHost.HubPipeline.AddModule(new IntegrationPointDataHubErrorHandlingPipelineModule());
			app.MapSignalR();
        }
    }
}