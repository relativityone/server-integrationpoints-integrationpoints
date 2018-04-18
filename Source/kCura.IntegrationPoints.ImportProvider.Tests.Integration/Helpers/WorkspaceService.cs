using kCura.IntegrationPoint.Tests.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers
{
	public static class WorkspaceService
	{
		private static readonly string _TEMPLATE_WORKSPACE_NAME = "Relativity Starter Template";

		public static int CreateWorkspace(string name)
		{
			return Workspace.CreateWorkspace(name, _TEMPLATE_WORKSPACE_NAME);
		}

		public static void DeleteWorkspace(int artifactId)
		{
			if (artifactId == 0)
			{
				return;
			}

			using (var rsApiClient = Rsapi.CreateRsapiClient())
			{
				rsApiClient.Repositories.Workspace.DeleteSingle(artifactId);
			}
		}
	}
}
