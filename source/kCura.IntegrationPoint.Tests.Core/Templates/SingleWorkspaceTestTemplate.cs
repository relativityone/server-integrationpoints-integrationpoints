using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	[TestFixture]
	[Category("Integration Tests")]
	public class SingleWorkspaceTestTemplate : IntegrationTestBase
	{
		private readonly string _name;
		protected int WorkspaceArtifactId { get; private set; }

		protected SingleWorkspaceTestTemplate(string name)
		{
			_name = name;
		}

		[TestFixtureSetUp]
		public virtual void SetUp()
		{
			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
			const string template = "New Case Template";
			WorkspaceArtifactId = Workspace.CreateWorkspace(_name, template);

			Install();
		}

		[TestFixtureSetUp]
		public virtual void TearDown()
		{
			Workspace.DeleteWorkspace(WorkspaceArtifactId);
		}


		protected virtual void Install()
		{
			Container.Register(Component.For<IHelper>().UsingFactoryMethod(k => Helper, managedExternally: true));
			Container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IHelper helper = k.Resolve<IHelper>();
					return new TestServiceContextHelper(helper, WorkspaceArtifactId);
				}));
			Container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestyleTransient());
			Container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());
			Container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceContext>()
					.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(WorkspaceArtifactId)))
					.LifeStyle.Transient);

			Container.Register(
				Component.For<IRSAPIClient>()
				.UsingFactoryMethod(k =>
				{
					IRSAPIClient client = Rsapi.CreateRsapiClient();
					client.APIOptions.WorkspaceID = WorkspaceArtifactId;
					return client;
				})
				.LifeStyle.Transient);

			Container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(k => Helper.GetServicesManager()));
			Container.Register(Component.For<IQueueRepository>().ImplementedBy<QueueRepository>().LifestyleTransient());

			var dependencies = new IWindsorInstaller[] { new QueryInstallers(), new KeywordInstaller(), new ServicesInstaller() };
			foreach (IWindsorInstaller dependency in dependencies)
			{
				dependency.Install(Container, ConfigurationStore);
			}
		}
	}
}