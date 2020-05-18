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

		public FieldsMappingValidator(IFieldsClassifyRunnerFactory fieldsClassifyRunnerFactory)
		{
			_fieldsClassifyRunnerFactory = fieldsClassifyRunnerFactory;
		}

		public async Task<FieldMappingValidationResult> ValidateAsync(IEnumerable<FieldMap> map, int sourceWorkspaceID, int destinationWorkspaceID)
		{
			FieldMappingValidationResult result = new FieldMappingValidationResult();

			List<FieldMap> mappedFields = map?.ToList();

			if (mappedFields == null || !mappedFields.Any())
			{
				return result;
			}

			IList<string> sourceMappedArtifactsIDs = mappedFields.Select(x => x.SourceField?.FieldIdentifier).ToList();
			IFieldsClassifierRunner sourceClassifierRunner = _fieldsClassifyRunnerFactory.CreateForSourceWorkspace();
			Dictionary<string, FieldClassificationResult> sourceFields = (await sourceClassifierRunner.ClassifyFieldsAsync(sourceMappedArtifactsIDs, sourceWorkspaceID).ConfigureAwait(false))
				.ToDictionary(f => f.FieldInfo.FieldIdentifier, f => f);

			IList<string> destinationMappedArtifactsIDs = mappedFields.Select(x => x.DestinationField?.FieldIdentifier).ToList();
			IFieldsClassifierRunner destinationClassifierRunner = _fieldsClassifyRunnerFactory.CreateForDestinationWorkspace();
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

				if (!IsFieldMapValid(sourceField, destinationField, fieldMap.FieldMapType))
				{
					result.InvalidMappedFields.Add(fieldMap);
				}
			}

			return result;
		}

		private bool IsFieldMapValid(FieldClassificationResult sourceField, FieldClassificationResult destinationField, FieldMapTypeEnum mapType)
		{
			switch(mapType)
			{
				case FieldMapTypeEnum.Identifier:
					return sourceField.FieldInfo.IsIdentifier && destinationField.FieldInfo.IsIdentifier;
				case FieldMapTypeEnum.FolderPathInformation:
					return destinationField == null
						|| CanBeMapped(sourceField, destinationField);
				case FieldMapTypeEnum.None:
					return sourceField != null && destinationField != null
						&& CanBeMapped(sourceField, destinationField);
				default:
					return true;
			}
		}

		private bool CanBeMapped(FieldClassificationResult sourceField, FieldClassificationResult destinationField)
		{
			return sourceField.ClassificationLevel == ClassificationLevel.AutoMap
				&& destinationField.ClassificationLevel == ClassificationLevel.AutoMap
				&& sourceField.FieldInfo.IsTypeCompatible(destinationField.FieldInfo);
		}
	}
}
