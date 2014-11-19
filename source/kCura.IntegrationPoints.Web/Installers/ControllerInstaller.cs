using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web.Installers
{
	public class ControllerInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Classes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());
			container.Register(Component.For<ISessionService>().ImplementedBy<SessionService>());

			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod<IRSAPIClient>(
				(k) => ConnectionHelper.Helper().GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				);

			container.Register(Component.For<global::Relativity.API.IDBContext>().UsingFactoryMethod<global::Relativity.API.IDBContext>(
				(k) => ConnectionHelper.Helper().GetDBContext(k.Resolve<ISessionService>().WorkspaceID))
				);
		}
	}
}