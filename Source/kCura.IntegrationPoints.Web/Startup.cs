using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(kCura.IntegrationPoints.Web.Startup))]
namespace kCura.IntegrationPoints.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}