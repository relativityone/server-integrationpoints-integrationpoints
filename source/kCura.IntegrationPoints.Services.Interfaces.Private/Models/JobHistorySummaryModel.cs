namespace kCura.IntegrationPoints.Services.Interfaces.Private.Models
{
	public class JobHistorySummaryModel
	{
		public int TotalDocumentsPushed { get; set; }

		public JobHistoryModel[] JobHistories { get; set; }
	}
}
