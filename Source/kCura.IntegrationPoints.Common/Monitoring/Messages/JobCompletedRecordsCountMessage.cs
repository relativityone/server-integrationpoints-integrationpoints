namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
    public class JobCompletedRecordsCountMessage : JobMessageBase
    {
        public long CompletedRecordsCount { get; set; }
    }
}
