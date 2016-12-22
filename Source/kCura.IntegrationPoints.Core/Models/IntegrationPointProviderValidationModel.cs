using System;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Models
{
	public class IntegrationPointProviderValidationModel
	{
		public IntegrationPointProviderValidationModel()
		{
		}

		public IntegrationPointProviderValidationModel(IntegrationPointModelBase model)
		{
			FieldsMap = model.Map;
			SourceConfiguration = model.SourceConfiguration;
			DestinationConfiguration = model.Destination;
			SourceProviderArtifactId = model.SourceProvider;
			DestinationProviderArtifactId = model.DestinationProvider;
			Type = model.Type;
		}

		public int ArtifactId { get; set; }

		public int ArtifactTypeId { get; set; }

		public int Type { get; set; }

		public string FieldsMap { get; set; }

		public string SourceProviderIdentifier { get; set; }

		public int SourceProviderArtifactId { get; set; }

		public string SourceConfiguration { get; set; }

		public string DestinationProviderIdentifier { get; set; }

		public int DestinationProviderArtifactId { get; set; }

		public string DestinationConfiguration { get; set; }

		public string IntegrationPointTypeIdentifier { get; set; }
	}
}