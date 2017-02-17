namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public interface IDestinationWorkspaceParser
	{
		int GetWorkspaceArtifactId(string destinationWorkspace);
		string GetInstanceName(string destinationWorkspace);
		string[] GetElements(string destinationWorkspace);
	}
}