namespace Relativity.Sync.Executors
{
	internal interface ITagNameFormatter
	{
		string FormatWorkspaceDestinationTagName(string federatedInstanceName, string destinationWorkspaceName, int destinationWorkspaceArtifactId);
		string FormatSourceJobTagName(string jobHistoryName, int jobHistoryArtifactId);
		string CreateSourceCaseTagName(string instanceName, string sourceWorkspaceName, int workspaceArtifactId);
	}
}