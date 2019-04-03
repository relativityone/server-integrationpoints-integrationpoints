using System.Threading;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class WorkspaceNameValidator : IWorkspaceNameValidator
	{
		private const string _WORKSPACE_INVALID_NAME_CHAR = ";";

		private readonly ISyncLog _logger;

		public WorkspaceNameValidator(ISyncLog logger)
		{
			_logger = logger;
		}

		public bool Validate(string workspaceName, CancellationToken token)
		{
			_logger.LogVerbose("Validating workspace name: {workspaceName}", workspaceName);
			bool isWorkspaceNameValid = !workspaceName.Contains(_WORKSPACE_INVALID_NAME_CHAR);
			if (!isWorkspaceNameValid)
			{
				_logger.LogError("Invalid workspace name: {workspaceName} (ArtifactID: {workspaceArtifactId}", workspaceName);

			}
			return isWorkspaceNameValid;
		}
	}
}