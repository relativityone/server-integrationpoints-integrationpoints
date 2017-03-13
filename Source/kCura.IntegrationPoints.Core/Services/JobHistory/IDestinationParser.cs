namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public interface IDestinationParser
	{
		int GetArtifactId(string destinationWorkspace);
		string GetName(string destinationWorkspace);
		string[] GetElements(string destinationWorkspace);
	}
}