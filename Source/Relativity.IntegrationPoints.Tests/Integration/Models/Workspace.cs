namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class Workspace
	{
		public int ArtifactId { get; set; }
		public string Name { get; set; }

		public Workspace()
		{
			ArtifactId = Artifact.Next();
		}
	}
}
