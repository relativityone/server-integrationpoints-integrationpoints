using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
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

			container.Register(Component.For<ISessionService>().UsingFactoryMethod(k => SessionService.Session).LifestylePerWebRequest());
			container.Register(Component.For<IPermissionService>().ImplementedBy<PermissionService>().LifestyleTransient());
			container.Register(Component.For<WebClientFactory>().ImplementedBy<WebClientFactory>().LifestyleTransient());
			container.Register(Component.For<RdoSynchronizer>().ImplementedBy<RdoSynchronizer>().LifestyleTransient());
			container.Register(Component.For<kCura.Apps.Common.Utils.Serializers.ISerializer>().ImplementedBy<kCura.Apps.Common.Utils.Serializers.JSONSerializer>().LifestyleTransient());
			container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());
			container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestyleTransient());
			container.Register(Component.For<ICPHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestylePerWebRequest());
			container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForWeb>().LifestylePerWebRequest());
			container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestylePerWebRequest());
			container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());
			container.Register(Component.For<IJobService>().ImplementedBy<IJobService>().LifestyleTransient());
			container.Register(
				Component.For<Data.IWorkspaceDBContext>()
					.ImplementedBy<Data.WorkspaceContext>()
					.UsingFactoryMethod((k) => new WorkspaceContext(k.Resolve<WebClientFactory>().CreateDbContext()))
					.LifeStyle.Transient);

			container.AddFacility<TypedFactoryFacility>();
			container.Register(Component.For<IErrorFactory>().AsFactory().UsingFactoryMethod((k) => new ErrorFactory(container)));
			container.Register(Component.For<WebAPIFilterException>().ImplementedBy<WebAPIFilterException>());

			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod((k) =>
				k.Resolve<WebClientFactory>().CreateClient()).LifestyleTransient());

			container.Register(Component.For<IDBContext>().UsingFactoryMethod((k) =>
				k.Resolve<WebClientFactory>().CreateDbContext()).LifestyleTransient());

			container.Register(Component.For<GridModelFactory>().ImplementedBy<GridModelFactory>().LifestyleTransient());


			container.Register(
				Component.For<GetApplicationBinaries>()
					.ImplementedBy<GetApplicationBinaries>().DynamicParameters((k, d) => d["eddsDBcontext"] = ConnectionHelper.Helper().GetDBContext(-1))
					.LifeStyle.Transient);

			container.Register(Component.For<RelativityUrlHelper>().ImplementedBy<RelativityUrlHelper>().LifeStyle.Transient);

			container.Register(Component.For<IEncryptionManager>().ImplementedBy<DefaultEncryptionManager>().LifeStyle.Transient);
			container.Register(Component.For<WebAPILoginException>().ImplementedBy<WebAPILoginException>().LifeStyle.Transient); 
		}
	}
}