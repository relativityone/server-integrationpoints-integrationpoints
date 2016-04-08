﻿using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.CustomPages;
using Relativity.Toggles;
using Relativity.Toggles.Providers;
using IDBContext = Relativity.API.IDBContext;

namespace kCura.IntegrationPoints.Web.Installers
{
	public class ControllerInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Classes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceService>().ImplementedBy<ControllerCustomPageService>().LifestyleTransient());
			container.Register(Component.For<IWorkspaceService>().ImplementedBy<WebAPICustomPageService>().LifestyleTransient());

			container.Register(Component.For<IConfig>().Instance(Config.Instance));

			container.Register(Component.For<ISessionService>().UsingFactoryMethod(k => SessionService.Session).LifestylePerWebRequest());
			container.Register(Component.For<IPermissionService>().ImplementedBy<PermissionService>().LifestyleTransient());
			container.Register(Component.For<WebClientFactory>().ImplementedBy<WebClientFactory>().LifestyleTransient());
			container.Register(Component.For<kCura.Apps.Common.Utils.Serializers.ISerializer>().ImplementedBy<kCura.Apps.Common.Utils.Serializers.JSONSerializer>().LifestyleTransient());
			container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());
			container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestylePerWebRequest());
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

			container.Register(Component.For<IServicesMgr>().UsingFactoryMethod((k) =>
				k.Resolve<WebClientFactory>().CreateServicesMgr()).LifestyleTransient());

			container.Register(Component.For<GridModelFactory>().ImplementedBy<GridModelFactory>().LifestyleTransient());

			container.Register(
				Component.For<GetApplicationBinaries>()
					.ImplementedBy<GetApplicationBinaries>().DynamicParameters((k, d) => d["eddsDBcontext"] = ConnectionHelper.Helper().GetDBContext(-1))
					.LifeStyle.Transient);

			container.Register(Component.For<RelativityUrlHelper>().ImplementedBy<RelativityUrlHelper>().LifeStyle.Transient);

			container.Register(Component.For<IEncryptionManager>().ImplementedBy<DefaultEncryptionManager>().LifeStyle.Transient);
			container.Register(Component.For<WebAPILoginException>().ImplementedBy<WebAPILoginException>().LifeStyle.Transient);

			// TODO: we need to make use of an async GetDBContextAsync (pending Dan Wells' patch) -- biedrzycki: Feb 5th, 2016
			container.Register(Component.For<IToggleProvider>().Instance(new SqlServerToggleProvider(
				() =>
				{
					SqlConnection connection = ConnectionHelper.Helper().GetDBContext(-1).GetConnection(true);

					return connection;
				},
				async () =>
				{
					Task<SqlConnection> task = Task.Run(() =>
					{
						SqlConnection connection = ConnectionHelper.Helper().GetDBContext(-1).GetConnection(true);
						return connection;
					});

					return await task;
				})).LifestyleTransient());

			container.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>().LifestyleTransient());
			
			container.Register(Component.For<IWorkspaceRepository>()
					.ImplementedBy<RsapiWorkspaceRepository>()
					.UsingFactoryMethod((k) => k.Resolve<IRepositoryFactory>().GetWorkspaceRepository())
					.LifestyleTransient());
		}
	}
}