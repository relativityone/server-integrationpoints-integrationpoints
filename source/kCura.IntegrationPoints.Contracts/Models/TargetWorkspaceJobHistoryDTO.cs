namespace kCura.IntegrationPoints.Contracts.Models
{
	public class TargetWorkspaceJobHistoryDTO
	{
		public int SourceWorkspaceArtifactId { get; set; } 
		public int ArtifactId { get; set; }
		public string Name { get; set; }
		public int JobHistoryArtifactId { get; set; }
		public string JobHistoryName { get; set; }
	}
}