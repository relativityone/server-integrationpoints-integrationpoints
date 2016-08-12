namespace kCura.IntegrationPoints.Core.Models
{
	public class StoppableJobCollection
	{
		public int[] PendingJobArtifactIds { get; set; } 
		public int[] ProcessingJobArtifactIds { get; set; }

		public bool HasStoppableJobs
		{
			get
			{
				bool hasStoppableJobs = this.PendingJobArtifactIds?.Length > 0
										|| this.ProcessingJobArtifactIds?.Length > 0;

				return hasStoppableJobs;
			}
		}
	}
}