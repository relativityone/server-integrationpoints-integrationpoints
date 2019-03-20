﻿using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.Integration
{
	internal sealed class TagNameFormatterStub : ITagNameFormatter
	{
		public string FormatWorkspaceDestinationTagName(string federatedInstanceName, string destinationWorkspaceName, int destinationWorkspaceArtifactId)
		{
			return $"{federatedInstanceName} - {destinationWorkspaceName} - {destinationWorkspaceArtifactId}";
		}
	}
}