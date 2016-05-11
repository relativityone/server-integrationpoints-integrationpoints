using NUnit.Framework;

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
		}

		[TearDown]
		public virtual void TearDown()
		{
			GerronHelper.Workspace.DeleteWorkspace(SourecWorkspaceArtifactId);
			GerronHelper.Workspace.DeleteWorkspace(TargetWorkspaceArtifactId);
		}
	}
}