using Relativity.API;
using System.Threading;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class WorkspaceNameValidator : IWorkspaceNameValidator
	{
		private const string _WORKSPACE_INVALID_NAME_CHAR = ";";

		private readonly IAPILog _logger;

		public WorkspaceNameValidator(IAPILog logger)
		{
			_logger = logger;
		}

		public bool Validate(string workspaceName, int workspaceArtifactId, CancellationToken token)
		{
			_logger.LogInformation("Validating workspace name.");
			bool isWorkspaceNameValid = !workspaceName.Contains(_WORKSPACE_INVALID_NAME_CHAR);
			if (!isWorkspaceNameValid)
			{
				_logger.LogError("Invalid workspace name (ArtifactID: {workspaceArtifactId})", workspaceArtifactId);

			}
			return isWorkspaceNameValid;
		}
	}
}
