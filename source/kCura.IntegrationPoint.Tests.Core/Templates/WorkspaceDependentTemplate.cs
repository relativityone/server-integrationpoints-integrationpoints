using Castle.MicroKernel.Registration;
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
	public class WorkspaceDependentTemplate : IntegrationTestBase
	{
		private readonly string _sourceWorkspaceName;
		private readonly string _targetWorkspaceName;

		public int SourecWorkspaceArtifactId { get; private set; }
		public int TargetWorkspaceArtifactId { get; private set; }

		public WorkspaceDependentTemplate(string sourceWorkspaceName, string targetWorkspaceName)
		{
			_sourceWorkspaceName = sourceWorkspaceName;
			_targetWorkspaceName = targetWorkspaceName;
		}


		[SetUp]
		public virtual void SetUp()
		{
			const string template = "New Case Template";
			SourecWorkspaceArtifactId = GerronHelper.Workspace.CreateWorkspace(_sourceWorkspaceName, template);
			TargetWorkspaceArtifactId = GerronHelper.Workspace.CreateWorkspace(_targetWorkspaceName, template);
			GerronHelper.Workspace.ImportApplicationToWorkspace(SourecWorkspaceArtifactId, SharedVariables.RapFileLocation, true);
			Install();
		}

		protected void Install()
		{
			Container.Register(Component.For<IHelper>().UsingFactoryMethod(k => Helper, managedExternally: true));
			Container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IHelper helper = k.Resolve<IHelper>();
					return new TestServiceContextHelper(helper, SourecWorkspaceArtifactId);
				}));
			Container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestyleTransient());
			Container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());
			Container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceContext>()
					.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(SourecWorkspaceArtifactId)))
					.LifeStyle.Transient);

			Container.Register(
				Component.For<IRSAPIClient>()
				.UsingFactoryMethod(k =>
				{
					IRSAPIClient client = GerronHelper.Rsapi.CreateRsapiClient();
					client.APIOptions.WorkspaceID = SourecWorkspaceArtifactId;
					return client;
				})
				.LifeStyle.Transient);

			Container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(k => Helper.GetServicesManager()));
			Container.Register(Component.For<IPermissionRepository>().ImplementedBy<PermissionRepository>().LifestyleTransient());

			var dependencies = new IWindsorInstaller[]{ new QueryInstallers(), new KeywordInstaller(), new ServicesInstaller()};
			foreach (IWindsorInstaller dependency in dependencies)
			{
				dependency.Install(Container, ConfigurationStore);
			}
		}

		[TearDown]
		public virtual void TearDown()
		{
			GerronHelper.Workspace.DeleteWorkspace(SourecWorkspaceArtifactId);
			GerronHelper.Workspace.DeleteWorkspace(TargetWorkspaceArtifactId);
		}
	}
}