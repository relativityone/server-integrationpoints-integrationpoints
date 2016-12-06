using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Services.Interfaces.Private.Extensions
{
	public static class ProviderExtensions
	{
		public static ProviderModel ToModel(this SourceProvider rdo)
		{
			return new ProviderModel
			{
				Name = rdo.Name,
				ArtifactId = rdo.ArtifactId
			};
		}

		public static ProviderModel ToModel(this DestinationProvider rdo)
		{
			return new ProviderModel
			{
				Name = rdo.Name,
				ArtifactId = rdo.ArtifactId
			};
		}
	}
}