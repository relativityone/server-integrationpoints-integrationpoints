using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.AspNet.SignalR.Hubs;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Helpers;

namespace kCura.IntegrationPoints.Web.Installers
{
	public static class ControllersRegistration
	{
		public static IWindsorContainer AddControllers(this IWindsorContainer container)
		{
			container.Register(Classes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());
            ILiquidFormsHelper liquidFormsHelper = container.Resolve<ILiquidFormsHelper>();
            if (!liquidFormsHelper.IsLiquidForms(0).GetAwaiter().GetResult())
            {
                container.Register(Classes.FromThisAssembly().BasedOn<IHub>().LifestyleTransient());
			}
            container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());

			return container;
		}
	}
}