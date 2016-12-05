namespace kCura.IntegrationPoints.Services
{
	public class IntegrationPointModel
	{
		/// <summary>
		/// Name of the integration point object.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Artifact Id of the integration point object.
		/// </summary>
		public int ArtifactId { get; set; }

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