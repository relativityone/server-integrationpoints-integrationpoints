using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
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
			container.Register(Component.For<ICustomPageService>().ImplementedBy<ControllerCustomPageService>().LifestyleTransient());
			container.Register(Component.For<ICustomPageService>().ImplementedBy<WebAPICustomPageService>().LifestyleTransient());

			container.Register(Component.For<ISessionService>().ImplementedBy<SessionService>());

			container.Register(Component.For<RsapiClientFactory>().ImplementedBy<RsapiClientFactory>());

			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod<IRSAPIClient>(
				(k) => k.Resolve<RsapiClientFactory>().CreateClient())
				);

			container.Register(Component.For<global::Relativity.API.IDBContext>().UsingFactoryMethod<global::Relativity.API.IDBContext>(
				(k) => k.Resolve<RsapiClientFactory>().CreateDbContext()));

			container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());

			container.AddFacility<TypedFactoryFacility>();
			container.Register(Component.For<IErrorFactory>().AsFactory().UsingFactoryMethod((k) => new ErrorFactory(container)));
			container.Register(Component.For<WebAPIFilterException>().ImplementedBy<WebAPIFilterException>());



		}
	}
}