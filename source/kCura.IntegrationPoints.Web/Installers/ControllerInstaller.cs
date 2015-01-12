using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;
using Relativity.API;
using IDBContext = Relativity.API.IDBContext;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web.Installers
{
	public class ControllerInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Classes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceService>().ImplementedBy<ControllerCustomPageService>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceService>().ImplementedBy<WebAPICustomPageService>().LifestyleTransient());

			container.Register(Component.For<ISessionService>().UsingFactoryMethod(k=> SessionService.Session).LifestylePerWebRequest());

			container.Register(Component.For<WebClientFactory>().ImplementedBy<WebClientFactory>().LifestyleTransient());
			container.Register(Component.For<RdoSynchronizer>().ImplementedBy<RdoSynchronizer>().LifestyleTransient());
			container.Register(Component.For<kCura.Apps.Common.Utils.Serializers.ISerializer>().ImplementedBy<kCura.Apps.Common.Utils.Serializers.JSONSerializer>().LifestyleTransient());
			container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());

			container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestylePerWebRequest()); 
			container.Register(Component.For<ICPHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestylePerWebRequest());
			container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForWeb>().LifestylePerWebRequest());
			container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestylePerWebRequest());
			container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());

			container.AddFacility<TypedFactoryFacility>();
			container.Register(Component.For<IErrorFactory>().AsFactory().UsingFactoryMethod((k) => new ErrorFactory(container)));
			container.Register(Component.For<WebAPIFilterException>().ImplementedBy<WebAPIFilterException>());

			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod((k) =>
				k.Resolve<WebClientFactory>().CreateClient()).LifestyleTransient());

			container.Register(Component.For<IDBContext>().UsingFactoryMethod((k) =>
				k.Resolve<WebClientFactory>().CreateDbContext()).LifestyleTransient());

			container.Register(Component.For<GridModelFactory>().ImplementedBy<GridModelFactory>().LifestyleTransient());


			container.Register(Component.For<RelativityUrlHelper>().ImplementedBy<RelativityUrlHelper>().LifeStyle.Transient);
		}
	}
}