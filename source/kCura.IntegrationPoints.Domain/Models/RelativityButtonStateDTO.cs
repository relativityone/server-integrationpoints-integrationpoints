namespace kCura.IntegrationPoints.Domain.Models
{
	public class RelativityButtonStateDTO : ButtonStateDTO
	{
		public bool RetryErrorsButtonEnabled { get; set; }
		public bool ViewErrorsLinkEnabled { get; set; }
	}
}