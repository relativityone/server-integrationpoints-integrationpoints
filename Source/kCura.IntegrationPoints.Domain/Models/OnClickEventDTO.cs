namespace kCura.IntegrationPoints.Domain.Models
{
    public class OnClickEventDTO
    {
        public string RunOnClickEvent { get; set; }

        public string StopOnClickEvent { get; set; }

        public string RetryErrorsOnClickEvent { get; set; }

        public string ViewErrorsOnClickEvent { get; set; }

        public string SaveAsProfileOnClickEvent { get; set; }

        public string DownloadErrorFileOnClickEvent { get; set; }
    }
}
