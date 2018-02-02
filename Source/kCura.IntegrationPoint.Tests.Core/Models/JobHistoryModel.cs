namespace kCura.IntegrationPoint.Tests.Core.Models
{
	using System;

	public class JobHistoryModel
	{
		public int JobId { get; set; }
		public DateTime StartTime { get; set; }
		public int ArtifactId { get; set; }
		public string Name { get; set; }
		public string IntegrationPoint { get; set; }
		public string JobType { get; set; }
		public string JobStatus { get; set; }
		public string DestinationWorkspace { get; set; }
		public string DestinationInstance { get; set; }
		public int ItemsTransferred { get; set; }
		public int TotalItems { get; set; }
		public int ItemsWithErrors { get; set; }
	}
}