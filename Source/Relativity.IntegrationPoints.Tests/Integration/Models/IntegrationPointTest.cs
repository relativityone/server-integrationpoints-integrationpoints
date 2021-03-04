namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class IntegrationPointTest
	{
		public int ArtifactId { get; set; }

		public int WorkspaceId { get; set; }

		public IntegrationPointTest()
		{
			ArtifactId = Artifact.NextId();
		}
	}
}
