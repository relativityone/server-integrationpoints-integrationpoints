namespace kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages
{
	public class JobTotalRecordsCountMessage : JobMessageBase
	{
		public long TotalRecordsCount { get; set; }
	}
}