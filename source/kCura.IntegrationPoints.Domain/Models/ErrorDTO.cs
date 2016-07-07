namespace kCura.IntegrationPoints.Domain.Models
{
	public class ErrorDTO : BaseDTO
	{
		/// <summary>
		/// The short and descriptive error message
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The full text for the error
		/// </summary>
		public string FullText { get;set; }
	}
}