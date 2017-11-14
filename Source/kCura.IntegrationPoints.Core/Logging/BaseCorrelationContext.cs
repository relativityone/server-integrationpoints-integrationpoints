namespace kCura.IntegrationPoints.Core.Logging
{
	public class BaseCorrelationContext
	{
		public string ActionName { get; set; }
		public int? WorkspaceId { get; set; }
		public int? UserId { get; set; }
	}
}
