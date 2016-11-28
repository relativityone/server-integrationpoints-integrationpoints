using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class IntegrationModelValidation
	{
		public IntegrationModelValidation()
		{
		}

		public IntegrationModelValidation(IntegrationPointModel model, string sourceProviderId, string destinationProviderId)
		{
			SourceProviderId = sourceProviderId;
			DestinationProviderId = destinationProviderId;
			FieldsMap = model.Map;
			SourceConfiguration = model.SourceConfiguration;
			DestinationConfiguration = model.Destination;
		}

		// TODO: move this field to the ExportSettings (SourceConfiguration)
		public int ArtifactTypeId { get; set; }

		public string FieldsMap { get; set; }

		public string SourceProviderId { get; set; }

		public string DestinationProviderId { get; set; }

		public string SourceConfiguration { get; set; }

		public string DestinationConfiguration { get; set; }
	}
}