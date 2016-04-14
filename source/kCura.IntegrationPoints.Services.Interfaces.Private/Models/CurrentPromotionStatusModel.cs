namespace kCura.IntegrationPoints.Services
{
	public class CurrentPromotionStatusModel
	{
		public int TotalDocumentsIncluded { get; set; }
		public int TotalDocumentsExcluded { get; set; }
		public int TotalDocumentsUntagged { get; set; }
		public int TotalDocumentsPushedToReview { get; set; }
	}
}
