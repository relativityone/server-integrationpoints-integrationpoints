namespace Relativity.Sync.Executors
{
	internal interface ITagNameFormatter
	{
		string FormatWorkspaceDestinationTagName(string federatedInstanceName, string destinationWorkspaceName, int destinationWorkspaceArtifactId);
	}
}