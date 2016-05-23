namespace kCura.IntegrationPoints.Contracts.Models
{
	public class PermissionCheckDTO
	{
		public bool Success { get; set; } 
		public string[] ErrorMessages { get; set; }
	}
}