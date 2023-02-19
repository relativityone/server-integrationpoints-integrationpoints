using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.AspNet.SignalR.Hubs;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Installers
{
    public static class ControllersRegistration
    {
        public static IWindsorContainer AddControllers(this IWindsorContainer container)
        {
            container.Register(Classes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());
            container.Register(Classes.FromThisAssembly().BasedOn<IHub>().LifestyleTransient());
            container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());

            return container;
        }
    }
}
