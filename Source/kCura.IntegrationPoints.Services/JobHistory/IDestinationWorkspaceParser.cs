namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IDestinationWorkspaceParser
	{
		int GetWorkspaceArtifactId(string destinationWorkspace);
	}
}