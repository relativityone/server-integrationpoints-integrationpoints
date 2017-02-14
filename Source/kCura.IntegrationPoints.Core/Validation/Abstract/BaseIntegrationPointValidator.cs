using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
	public abstract class BaseIntegrationPointValidator<TValidator> : IIntegrationPointValidator where TValidator : IValidator
	{
		protected readonly ILookup<string, TValidator> _validatorsMap;
		protected readonly ISerializer _serializer;

		public BaseIntegrationPointValidator(IEnumerable<TValidator> validators, ISerializer serializer)
		{
			_validatorsMap = validators.ToLookup(x => x.Key);
			_serializer = serializer;
		}

		public static string GetProviderValidatorKey(string sourceProviderId, string destinationProviderId)
		{
			sourceProviderId = sourceProviderId.ToUpper();
			destinationProviderId = destinationProviderId.ToUpper();

			return $"{sourceProviderId}+{destinationProviderId}";
		}

		public IntegrationPointProviderValidationModel CreateValidationModel(IntegrationPointModelBase model, SourceProvider sourceProvider,
			DestinationProvider destinationProvider, IntegrationPointType integrationPointType)
		{
			var destinationConfiguration = _serializer.Deserialize<ImportSettings>(model.Destination);

			return new IntegrationPointProviderValidationModel(model)
			{
				ArtifactId = model.ArtifactID,
				ArtifactTypeId = destinationConfiguration.ArtifactTypeId,
				SourceProviderIdentifier = sourceProvider.Identifier,
				SourceProviderArtifactId = sourceProvider.ArtifactId,
				SourceConfiguration = model.SourceConfiguration,
				DestinationProviderIdentifier = destinationProvider.Identifier,
				DestinationProviderArtifactId = destinationProvider.ArtifactId,
				DestinationConfiguration = model.Destination,
				FieldsMap = model.Map,
				Type = model.Type,
				IntegrationPointTypeIdentifier = integrationPointType.Identifier,
				SecuredConfiguration = model.SecuredConfiguration
			};
		}
			
		public abstract ValidationResult Validate(IntegrationPointModelBase model, SourceProvider sourceProvider,
			DestinationProvider destinationProvider, IntegrationPointType integrationPointType);
	}
}
