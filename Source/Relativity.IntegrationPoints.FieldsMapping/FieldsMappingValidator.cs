using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace Relativity.IntegrationPoints.FieldsMapping
{
    public class FieldsMappingValidator : IFieldsMappingValidator
    {
        private readonly IFieldsClassifyRunnerFactory _fieldsClassifyRunnerFactory;

        private readonly IList<string> _unicodeDependentFieldsTypes = new List<string>()
        {
            FieldTypeName.FIXED_LENGTH_TEXT,
            FieldTypeName.LONG_TEXT,
            FieldTypeName.SINGLE_CHOICE,
            FieldTypeName.MULTIPLE_CHOICE
        };

        public FieldsMappingValidator(IFieldsClassifyRunnerFactory fieldsClassifyRunnerFactory)
        {
            _fieldsClassifyRunnerFactory = fieldsClassifyRunnerFactory;
        }

        public async Task<FieldMappingValidationResult> ValidateAsync(IEnumerable<FieldMap> map, int sourceWorkspaceID, int destinationWorkspaceID, int sourceArtifactTypeId, int destinationArtifactTypeId)
        {
            FieldMappingValidationResult result = new FieldMappingValidationResult();

            List<FieldMap> mappedFields = map?.ToList();

            if (mappedFields == null || !mappedFields.Any())
            {
                return result;
            }

            IList<string> sourceMappedArtifactsIDs = mappedFields.Select(x => x.SourceField?.FieldIdentifier).ToList();
            IFieldsClassifierRunner sourceClassifierRunner = _fieldsClassifyRunnerFactory.CreateForSourceWorkspace(sourceArtifactTypeId);
            Dictionary<string, FieldClassificationResult> sourceFields = (await sourceClassifierRunner.ClassifyFieldsAsync(sourceMappedArtifactsIDs, sourceWorkspaceID).ConfigureAwait(false))
                .ToDictionary(f => f.FieldInfo.FieldIdentifier, f => f);

            IList<string> destinationMappedArtifactsIDs = mappedFields.Select(x => x.DestinationField?.FieldIdentifier).ToList();
            IFieldsClassifierRunner destinationClassifierRunner = _fieldsClassifyRunnerFactory.CreateForDestinationWorkspace(destinationArtifactTypeId);
            Dictionary<string, FieldClassificationResult> destinationFields = (await destinationClassifierRunner.ClassifyFieldsAsync(destinationMappedArtifactsIDs, destinationWorkspaceID).ConfigureAwait(false))
                .ToDictionary(f => f.FieldInfo.FieldIdentifier, f => f);

            foreach (FieldMap fieldMap in mappedFields)
            {
                string sourceFieldFieldIdentifier = fieldMap.SourceField.FieldIdentifier ?? string.Empty;
                string destinationFieldFieldIdentifier = fieldMap.DestinationField.FieldIdentifier ?? string.Empty;

                FieldClassificationResult sourceField = sourceFields.ContainsKey(sourceFieldFieldIdentifier) ? sourceFields[sourceFieldFieldIdentifier] : null;
                FieldClassificationResult destinationField = destinationFields.ContainsKey(destinationFieldFieldIdentifier) ? destinationFields[destinationFieldFieldIdentifier] : null;

                if (fieldMap.FieldMapType == FieldMapTypeEnum.Identifier)
                {
                    result.IsObjectIdentifierMapValid = sourceField.FieldInfo.IsTypeCompatible(destinationField.FieldInfo);
                }

                var fieldMapValidation = IsFieldMapValid(sourceField, destinationField, fieldMap.FieldMapType);
                if (!fieldMapValidation.IsValid)
                {
                    result.InvalidMappedFields.Add(new InvalidFieldMap()
                    {
                        FieldMap = fieldMap,
                        InvalidReasons = fieldMapValidation.Reasons
                    });
                }
            }

            return result;
        }

        private MapValidationResult IsFieldMapValid(FieldClassificationResult sourceField, FieldClassificationResult destinationField, FieldMapTypeEnum mapType)
        {
            switch(mapType)
            {
                case FieldMapTypeEnum.Identifier:
                    return sourceField.FieldInfo.IsIdentifier && destinationField.FieldInfo.IsIdentifier
                        ? MapValidationResult.Valid : new MapValidationResult(InvalidMappingReasons._FIELD_IDENTIFIERS_NOT_MAPPED);
                case FieldMapTypeEnum.FolderPathInformation:
                    return destinationField == null ? MapValidationResult.Valid : CanBeMapped(sourceField, destinationField);
                case FieldMapTypeEnum.None:
                    return CanBeMapped(sourceField, destinationField);
                default:
                    return MapValidationResult.Valid;
            }
        }

        private MapValidationResult CanBeMapped(FieldClassificationResult sourceField, FieldClassificationResult destinationField)
        {
            var result = MapValidationResult.Valid;

            if(sourceField == null || destinationField == null)
            {
                result.AddInvalidReason(InvalidMappingReasons._FIELD_IS_MISSING);
                return result;
            }

            bool unsupported = sourceField.ClassificationLevel == ClassificationLevel.AutoMap 
                && destinationField.ClassificationLevel == ClassificationLevel.AutoMap;
            if (!unsupported)
            {
                result.AddInvalidReason(InvalidMappingReasons._UNSUPORTED_TYPES);
            }

            if(!sourceField.FieldInfo.IsTypeCompatible(destinationField.FieldInfo))
            {
                result.AddInvalidReason(InvalidMappingReasons._INCOMPATIBLE_TYPES);
            }

            if(!UnicodeIsSame(sourceField, destinationField))
            {
                result.AddInvalidReason(InvalidMappingReasons._UNICODE_DIFFERENCE);
            }

            return result;
        }

        private bool UnicodeIsSame(FieldClassificationResult sourceField, FieldClassificationResult destinationField)
            => !_unicodeDependentFieldsTypes.Contains(sourceField.FieldInfo.Type) || sourceField.FieldInfo.Unicode == destinationField.FieldInfo.Unicode;

        private class MapValidationResult
        {
            public IList<string> Reasons { get; set; } = new List<string>();
            public bool IsValid => !Reasons.Any();

            private MapValidationResult() { }

            public MapValidationResult(string reason)
            {
                Reasons.Add(reason);
            }

            public void AddInvalidReason(string reason) => Reasons.Add(reason);

            public static MapValidationResult Valid => new MapValidationResult();
        }
    }
}
