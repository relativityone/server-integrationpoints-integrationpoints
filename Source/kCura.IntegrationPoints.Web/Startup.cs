using kCura.IntegrationPoints.Web.Helpers;
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

            ILiquidFormsHelper liquidFormsHelper = new LiquidFormsHelper(helper.GetServicesManager(), logger);
            bool isLiquidForms = liquidFormsHelper.IsLiquidForms(0).GetAwaiter().GetResult();
            if (!isLiquidForms)
            {
                GlobalHost.HubPipeline.AddModule(new IntegrationPointDataHubErrorHandlingPipelineModule(logger));
                app.MapSignalR();
            }
        }
    }
}