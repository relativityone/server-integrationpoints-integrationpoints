﻿using System.Threading;

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

		public bool Validate(string workspaceName, int workspaceArtifactId, CancellationToken token)
		{
			_logger.LogVerbose("Validating workspace name.");
			bool isWorkspaceNameValid = !workspaceName.Contains(_WORKSPACE_INVALID_NAME_CHAR);
			if (!isWorkspaceNameValid)
			{
				_logger.LogError("Invalid workspace name (ArtifactID: {workspaceArtifactId})", workspaceArtifactId);

			}
			return isWorkspaceNameValid;
		}
	}
}