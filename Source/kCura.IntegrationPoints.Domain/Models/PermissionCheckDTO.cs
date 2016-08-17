namespace kCura.IntegrationPoints.Domain.Models
{
	public class PermissionCheckDTO
	{
		public bool Success { get; set; } 
		public string[] ErrorMessages { get; set; }
	}
}