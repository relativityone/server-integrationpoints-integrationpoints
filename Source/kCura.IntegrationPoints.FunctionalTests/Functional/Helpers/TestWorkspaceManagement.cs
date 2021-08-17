using System.Collections.Generic;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
	internal class TestWorkspaceManagement
	{
		public Dictionary<string, Workspace> Workspaces { get; }

		public TestWorkspaceManagement()
		{
			Workspaces = new Dictionary<string, Workspace>();
		}

		public Workspace CreateWorkspace(string workspaceName, string templateWorkspaceName)
		{
			Workspace workspace = RelativityFacade.Instance.CreateWorkspace(workspaceName, templateWorkspaceName);

			Workspaces.Add(workspaceName, workspace);

			return workspace;
		}

		public void DeletaAll()
		{
			foreach (var workspace in Workspaces)
			{
				RelativityFacade.Instance.DeleteWorkspace(workspace.Value);
			}
		}
	}
}
