namespace kCura.IntegrationPoints.Domain.Models
{
	public class RelativityOnClickEventDTO : OnClickEventDTO
	{
		public string RetryErrorsOnClickEvent { get; set; }
		public string ViewErrorsOnClickEvent { get; set; }
	}
}
