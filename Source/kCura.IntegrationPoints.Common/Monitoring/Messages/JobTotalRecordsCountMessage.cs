namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
    public class JobTotalRecordsCountMessage : JobMessageBase
    {
        public long TotalRecordsCount { get; set; }
    }
}