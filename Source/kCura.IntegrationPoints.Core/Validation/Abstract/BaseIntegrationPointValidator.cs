using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
    public abstract class BaseIntegrationPointValidator<TValidator> : IIntegrationPointValidator where TValidator : IValidator
    {
        protected readonly ILookup<string, TValidator> _validatorsMap;
        protected readonly ISerializer _serializer;

        protected BaseIntegrationPointValidator(IEnumerable<TValidator> validators, ISerializer serializer)
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
            IntegrationPointDtoBase model,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            IntegrationPointType integrationPointType,
            Guid objectTypeGuid,
            int userId)
        {
            return new IntegrationPointProviderValidationModel(model)
            {
                ArtifactId = model.ArtifactId,
                ArtifactTypeId = model.DestinationConfiguration.ArtifactTypeId,
                UserId = userId,
                SourceProviderIdentifier = sourceProvider.Identifier,
                SourceProviderArtifactId = sourceProvider.ArtifactId,
                SourceConfiguration = model.SourceConfiguration,
                DestinationProviderIdentifier = destinationProvider.Identifier,
                DestinationProviderArtifactId = destinationProvider.ArtifactId,
                DestinationConfiguration = model.DestinationConfiguration,
                FieldsMap = model.FieldMappings,
                Type = model.Type,
                IntegrationPointTypeIdentifier = integrationPointType.Identifier,
                ObjectTypeGuid = objectTypeGuid,
                SecuredConfiguration = model.SecuredConfiguration,
                CreateSavedSearch = model.DestinationConfiguration.CreateSavedSearchForTagging
            };
        }

        public abstract ValidationResult Validate(
            IntegrationPointDtoBase model,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            IntegrationPointType integrationPointType,
            Guid objectTypeGuid,
            int userId);
    }
}
