namespace kCura.IntegrationPoints.Core.Models
{
	public class StoppableJobCollection
	{
		public int[] PendingJobArtifactIds { get; set; } 
		public int[] ProcessingJobArtifactIds { get; set; }
	}
}