namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class IntegrationPoint
	{
		public int ArtifactId { get; set; }

		public int WorkspaceId { get; set; }

		public IntegrationPoint()
		{
			ArtifactId = Artifact.Next();
		}
	}
}
