namespace kCura.IntegrationPoints.Domain.Models
{
	public class PermissionCheckDTO
	{
		public bool Success => ErrorMessages == null || ErrorMessages.Length == 0;

		public string[] ErrorMessages { get; set; }
	}
}