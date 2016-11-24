using System;
using System.Text;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class WorkspaceNameValidator : IWorkspaceNameValidator
	{
		private readonly IHelper _helper;

		public WorkspaceNameValidator(IHelper helper)
		{
			_helper = helper;
		}

		public string Validate(string sourceConfiguration)
		{
			var sourceSettings = JsonConvert.DeserializeObject<IntegrationPointSourceConfiguration>(sourceConfiguration);

			StringBuilder errorMessage = new StringBuilder();

			try
			{
				using (IRSAPIClient client = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					string targetWorkspace = RetrieveWorkspaceName(client, sourceSettings.TargetWorkspaceArtifactId);
					string sourceWorkspace = RetrieveWorkspaceName(client, sourceSettings.SourceWorkspaceArtifactId);

					if (targetWorkspace.Contains(";"))
					{
						errorMessage.Append("Destination workspace name contains an invalid character. Please remove before continuing.</br>");
					}
					if (sourceWorkspace.Contains(";"))
					{
						errorMessage.Append("Source workspace name contains an invalid character. Please remove before continuing.</br>");
					}
				}
			}
			catch (Exception e)
			{
				_helper.GetLoggerFactory().GetLogger().ForContext<WorkspaceNameValidator>().LogError(e, "Failed to validate workspaces' names.");
			}

			return errorMessage.ToString();
		}

		private string RetrieveWorkspaceName(IRSAPIClient client, int workspaceId)
		{
			var result = client.Repositories.Workspace.Read(workspaceId);
			return result.Results[0].Artifact.Name;
		}

		internal class IntegrationPointSourceConfiguration
		{
			public int SourceWorkspaceArtifactId;
			public int TargetWorkspaceArtifactId;
		}
	}
}