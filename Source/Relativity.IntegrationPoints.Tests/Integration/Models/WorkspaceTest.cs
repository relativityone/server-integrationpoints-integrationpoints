namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class WorkspaceTest
	{
		public int ArtifactId { get; set; }
		public string Name { get; set; }

		public WorkspaceTest()
		{
			ArtifactId = Artifact.NextId();
		}
	}
}
