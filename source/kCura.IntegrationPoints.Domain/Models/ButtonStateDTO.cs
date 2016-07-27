namespace kCura.IntegrationPoints.Domain.Models
{
	public class ButtonStateDTO
	{
		public bool RunNowButtonEnabled { get; set; }
		public bool RetryErrorsButtonEnabled { get; set; }
		public bool ViewErrorsLinkEnabled { get; set; }
		public bool StopButtonEnabled { get; set; }
	}
}
