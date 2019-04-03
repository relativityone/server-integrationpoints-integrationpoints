using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

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

		public async Task<bool> ValidateWorkspaceNameAsync(IProxyFactory proxyFactory, int workspaceArtifactId, CancellationToken token)
		{
			try
			{
				string workspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(proxyFactory, workspaceArtifactId, token).ConfigureAwait(false);
				_logger.LogVerbose("Validating workspace name: {workspaceName}", workspaceName);
				bool isWorkspaceNameValid = !workspaceName.Contains(_WORKSPACE_INVALID_NAME_CHAR);
				if (!isWorkspaceNameValid)
				{
					_logger.LogError("Invalid workspace name: {workspaceName} (ArtifactID: {workspaceArtifactId}", workspaceName, workspaceArtifactId);

				}
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