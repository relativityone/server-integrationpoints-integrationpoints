using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class WorkspaceNameValidator : IWorkspaceNameValidator
	{
		private const string _WORKSPACE_INVALID_NAME_CHAR = ";";

		private readonly IWorkspaceNameQuery _workspaceNameQuery;
		private readonly ISyncLog _logger;

		public WorkspaceNameValidator(IWorkspaceNameQuery workspaceNameQuery, ISyncLog logger)
		{
			_workspaceNameQuery = workspaceNameQuery;
			_logger = logger;
		}

		public async Task<bool> ValidateWorkspaceNameAsync(int workspaceArtifactId, CancellationToken token)
		{
			try
			{
				string workspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(workspaceArtifactId, token).ConfigureAwait(false);
				_logger.LogVerbose($"Validating workspace name: {workspaceName}");
				bool isWorkspaceNameValid = !workspaceName.Contains(_WORKSPACE_INVALID_NAME_CHAR);
				return isWorkspaceNameValid;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while querying for workspace artifact ID: {workspaceArtifactId}", workspaceArtifactId);
				return false;
			}
		}
	}
}