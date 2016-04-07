namespace kCura.IntegrationPoints.Services.Interfaces.Private.Models
{
	public class CurrentSnapshotModel
	{
		public int TotalDocumentsIncluded { get; set; }
		public int TotalDocumentsExcluded { get; set; }
		public int TotalDocumentsUntagged { get; set; }
		public int TotalDocumentsPushedToReview { get; set; }
	}
}
