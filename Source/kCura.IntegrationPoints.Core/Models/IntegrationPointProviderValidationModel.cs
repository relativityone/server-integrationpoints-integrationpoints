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
		}

		public int ArtifactTypeId { get; set; }

		public string FieldsMap { get; set; }

		public string SourceProviderIdentifier { get; set; }

		public string SourceConfiguration { get; set; }

		public string DestinationProviderIdentifier { get; set; }

		public string DestinationConfiguration { get; set; }

		public IntegrationPointModel ConvertToIpModel()
		{
			return new IntegrationPointModel
			{
				Destination = DestinationConfiguration,
				SourceConfiguration = SourceConfiguration,
				Map = FieldsMap,
			};
		}
	}
}