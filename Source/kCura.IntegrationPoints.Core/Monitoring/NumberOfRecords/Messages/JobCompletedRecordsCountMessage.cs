namespace kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages
{
	public class JobCompletedRecordsCountMessage : JobMessageBase
	{
		public long CompletedRecordsCount { get; set; }
	}
}