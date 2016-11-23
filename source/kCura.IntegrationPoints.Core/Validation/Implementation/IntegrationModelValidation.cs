using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class IntegrationModelValidation
	{
		public IntegrationModelValidation(){}

		public IntegrationModelValidation(IntegrationModel model, string sourceProviderId, string destinationProviderId)
		{
			SourceProviderId = sourceProviderId;
			DestinationProviderId = destinationProviderId;
			FieldsMap = model.Map;
			SourceConfiguration = model.SourceConfiguration;
			DestinationConfiguration = model.Destination;
		}

		public string FieldsMap { get; set; }

		public string SourceProviderId { get; set; }

		public string DestinationProviderId { get; set; }

		public string SourceConfiguration { get; set; }
		public string DestinationConfiguration { get; set; }
	}
}
