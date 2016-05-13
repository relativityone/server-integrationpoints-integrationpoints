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
	[Explicit]
	public class WorkspaceDependentTemplate : IntegrationTestBase
	{
		private readonly string _sourceWorkspaceName;
		private readonly string _targetWorkspaceName;

		public int SourceWorkspaceArtifactId { get; private set; }
		public int TargetWorkspaceArtifactId { get; private set; }
		public int SavedSearchArtifactId { get; set; }

		public WorkspaceDependentTemplate(string sourceWorkspaceName, string targetWorkspaceName)
		{
			_sourceWorkspaceName = sourceWorkspaceName;
			_targetWorkspaceName = targetWorkspaceName;
		}

		[SetUp]
		public virtual void SetUp()
		{
			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
			const string template = "New Case Template";
			SourceWorkspaceArtifactId = GerronHelper.Workspace.CreateWorkspace(_sourceWorkspaceName, template);
			TargetWorkspaceArtifactId = SourceWorkspaceArtifactId;
			GerronHelper.Workspace.ImportApplicationToWorkspace(SourceWorkspaceArtifactId, SharedVariables.RapFileLocation, true);
			SavedSearchArtifactId = GerronHelper.SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, "All documents");
			Install();
		}

		protected void Install()
		{
			Container.Register(Component.For<IHelper>().UsingFactoryMethod(k => Helper, managedExternally: true));
			Container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IHelper helper = k.Resolve<IHelper>();
					return new TestServiceContextHelper(helper, SourceWorkspaceArtifactId);
				}));
			Container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestyleTransient());
			Container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());
			Container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceContext>()
					.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(SourceWorkspaceArtifactId)))
					.LifeStyle.Transient);

			Container.Register(
				Component.For<IRSAPIClient>()
				.UsingFactoryMethod(k =>
				{
					IRSAPIClient client = GerronHelper.Rsapi.CreateRsapiClient();
					client.APIOptions.WorkspaceID = SourceWorkspaceArtifactId;
					return client;
				})
				.LifeStyle.Transient);

			Container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(k => Helper.GetServicesManager()));
			Container.Register(Component.For<IPermissionRepository>().UsingFactoryMethod( k => Helper.PermissionManager));

			var dependencies = new IWindsorInstaller[]{ new QueryInstallers(), new KeywordInstaller(), new ServicesInstaller()};
			foreach (IWindsorInstaller dependency in dependencies)
			{
				dependency.Install(Container, ConfigurationStore);
			}
		}

		[TearDown]
		public virtual void TearDown()
		{
			//GerronHelper.Workspace.DeleteWorkspace(SourceWorkspaceArtifactId);
			//GerronHelper.Workspace.DeleteWorkspace(TargetWorkspaceArtifactId);
		}
	}
}