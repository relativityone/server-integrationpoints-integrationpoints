using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices
{
    public class EntityFullNameService : IEntityFullNameService
    {
        private readonly IEntityFullNameObjectManagerService _entityFullNameObjectManagerService;
        private readonly IAPILog _logger;

        public EntityFullNameService(IEntityFullNameObjectManagerService entityFullNameObjectManagerService, IAPILog logger)
        {
            _entityFullNameObjectManagerService = entityFullNameObjectManagerService;
            _logger = logger;
        }

        public string FormatFullName(Dictionary<string, IndexedFieldMap> destinationFieldNameToFieldMapDictionary, IDataReader reader)
        {
            FieldEntry firstNameField = destinationFieldNameToFieldMapDictionary[EntityFieldNames.FirstName].FieldMap.SourceField;
            FieldEntry lastNameField = destinationFieldNameToFieldMapDictionary[EntityFieldNames.LastName].FieldMap.SourceField;

            string firstName = reader[firstNameField.ActualName]?.ToString() ?? string.Empty;
            string lastName = reader[lastNameField.ActualName]?.ToString() ?? string.Empty;

            string fullName = FormatFullName(firstName, lastName);

            return fullName;
        }

        public async Task<bool> ShouldHandleFullNameAsync(CustomProviderDestinationConfiguration configuration, List<IndexedFieldMap> fieldsMap)
        {
            bool isFullNameMapped = fieldsMap.Exists(x => x.DestinationFieldName == EntityFieldNames.FullName);
            bool isEntity = await _entityFullNameObjectManagerService.IsEntityAsync(configuration.CaseArtifactId, configuration.ArtifactTypeId).ConfigureAwait(false);
            bool isOverlayWithCustomField = configuration.ImportOverwriteMode == ImportOverwriteModeEnum.OverlayOnly && configuration.OverlayIdentifier != EntityFieldNames.FullName;

            bool shouldHandleFullName = isEntity && !isFullNameMapped && !isOverlayWithCustomField;

            _logger.LogInformation(
                "Should handle full name: {shouldHandleFullName} because is Full Name field mapped: {isFullNameMapped}, is Entity: {isEntity}, OverlayWithCustomField: {overlayWithCustomField}",
                shouldHandleFullName,
                isFullNameMapped,
                isEntity,
                isOverlayWithCustomField);

            return shouldHandleFullName;
        }

        public async Task EnrichFieldMapWithFullNameAsync(IntegrationPointInfo integrationPoint)
        {
            _logger.LogInformation("Enriching Fields Mapping with Full Name field...");

            int fullNameArtifactId = await _entityFullNameObjectManagerService.GetFullNameArtifactId(integrationPoint.DestinationConfiguration.CaseArtifactId).ConfigureAwait(false);

            var fullNameField = new FieldEntry
            {
                DisplayName = EntityFieldNames.FullName,
                IsIdentifier = true,
                IsRequired = false,
                FieldIdentifier = fullNameArtifactId.ToString()
            };

            FieldMap fullNameFieldMap = new FieldMap
            {
                FieldMapType = FieldMapTypeEnum.None,
                SourceField = fullNameField,
                DestinationField = fullNameField
            };

            IndexedFieldMap indexedFullNameFieldMap = new IndexedFieldMap(fullNameFieldMap, FieldMapType.EntityFullName, integrationPoint.FieldMap.Count);

            integrationPoint.FieldMap.Add(indexedFullNameFieldMap);
        }

        private static string FormatFullName(string firstName, string lastName)
        {
            string fullName = lastName;

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    fullName += ", ";
                }

                fullName += firstName;
            }

            return fullName;
        }
    }
}
