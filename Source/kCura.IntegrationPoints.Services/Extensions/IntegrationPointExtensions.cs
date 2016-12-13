namespace kCura.IntegrationPoints.Services.Extensions
{
	public static class IntegrationPointExtensions
	{
		public static IntegrationPointModel ToIntegrationPointModel(this Data.IntegrationPoint data)
		{
			return new IntegrationPointModel()
			{
				ArtifactId = data.ArtifactId,
				Name = data.Name,
				SourceProvider = data.SourceProvider ?? 0,
				DestinationProvider = data.DestinationProvider ?? 0
			};
		}
	}
}