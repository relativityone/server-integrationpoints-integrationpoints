using System;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Helpers
{
	public class ApiHelper
	{
		/// <summary>
		/// Executes RSAPI client
		/// </summary>
		/// <param name="helper">IHelper object.</param>
		/// <param name="workspaceArtifactId">Workspace artifact id.</param>
		/// <param name="action"></param>
		/// <param name="executionIdentity">Execution identitiy (defaults to current user)</param>
		public static void ExecuteRsapi(IHelper helper, int workspaceArtifactId, Action<IRSAPIClient> action, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			using (IRSAPIClient rsapiClient = helper.GetServicesManager().CreateProxy<IRSAPIClient>(executionIdentity))
			{
				rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
				action(rsapiClient);
			}
		}
	}
}
