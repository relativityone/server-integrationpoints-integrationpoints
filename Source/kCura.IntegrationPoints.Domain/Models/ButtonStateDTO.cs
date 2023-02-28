namespace kCura.IntegrationPoints.Domain.Models
{
    public class ButtonStateDTO
    {
        public bool RunButtonEnabled { get; set; }

        public bool StopButtonEnabled { get; set; }

        public bool RetryErrorsButtonEnabled { get; set; }

        public bool RetryErrorsButtonVisible { get; set; }

        public bool ViewErrorsLinkEnabled { get; set; }

        public bool ViewErrorsLinkVisible { get; set; }

        public bool SaveAsProfileButtonVisible { get; set; }

        public bool DownloadErrorFileLinkEnabled { get; set; }

        public bool DownloadErrorFileLinkVisible { get; set; }

        public bool CalculateStatisticsButtonEnabled { get; set; }
    }
}
