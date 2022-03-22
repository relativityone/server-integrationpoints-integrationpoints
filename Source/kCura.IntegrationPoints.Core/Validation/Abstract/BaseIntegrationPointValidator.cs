using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
	public abstract class BaseIntegrationPointValidator<TValidator> : IIntegrationPointValidator where TValidator : IValidator
	{
		protected readonly ILookup<string, TValidator> _validatorsMap;
		protected readonly IIntegrationPointSerializer _serializer;

		protected BaseIntegrationPointValidator(IEnumerable<TValidator> validators, IIntegrationPointSerializer serializer)
		{
			_validatorsMap = validators.ToLookup(x => x.Key);
			_serializer = serializer;
		}

		public static string GetProviderValidatorKey(string sourceProviderId, string destinationProviderId)
		{
			sourceProviderId = sourceProviderId.ToUpperInvariant();
			destinationProviderId = destinationProviderId.ToUpperInvariant();

			return $"{sourceProviderId}+{destinationProviderId}";
		}

		public IntegrationPointProviderValidationModel CreateValidationModel(
			IntegrationPointModelBase model,
			SourceProvider sourceProvider,
			DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType,
			Guid objectTypeGuid,
			int userId)
		{
			ImportSettings destinationConfiguration = _serializer.Deserialize<ImportSettings>(model.Destination);

			return new IntegrationPointProviderValidationModel(model)
			{
				ArtifactId = model.ArtifactID,
				ArtifactTypeId = destinationConfiguration.ArtifactTypeId,
				UserId = userId,
				SourceProviderIdentifier = sourceProvider.Identifier,
				SourceProviderArtifactId = sourceProvider.ArtifactId,
				SourceConfiguration = model.SourceConfiguration,
				DestinationProviderIdentifier = destinationProvider.Identifier,
				DestinationProviderArtifactId = destinationProvider.ArtifactId,
				DestinationConfiguration = model.Destination,
				FieldsMap = model.Map,
				Type = model.Type,
				IntegrationPointTypeIdentifier = integrationPointType.Identifier,
				ObjectTypeGuid = objectTypeGuid,
				SecuredConfiguration = model.SecuredConfiguration,
				CreateSavedSearch = destinationConfiguration.CreateSavedSearchForTagging
			};
		}


		public abstract ValidationResult Validate(
			IntegrationPointModelBase model,
			SourceProvider sourceProvider,
			DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType,
			Guid objectTypeGuid,
			int userId);
	}
}
