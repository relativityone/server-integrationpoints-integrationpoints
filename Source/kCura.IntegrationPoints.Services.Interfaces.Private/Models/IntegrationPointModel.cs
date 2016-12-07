namespace kCura.IntegrationPoints.Services
{
	public class IntegrationPointModel : BaseModel
	{
		/// <summary>
		/// Artifact Id of the source provider.
		/// </summary>
		public int SourceProvider { get; set; }

		/// <summary>
		/// Artifact Id of the destination provider.
		/// </summary>
		public int DestinationProvider { get; set; }
	}
}