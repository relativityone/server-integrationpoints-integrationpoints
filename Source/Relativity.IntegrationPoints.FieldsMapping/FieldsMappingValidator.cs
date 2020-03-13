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

		public async Task<IEnumerable<FieldMap>> ValidateAsync(IEnumerable<FieldMap> map, int sourceWorkspaceID, int destinationWorkspaceID)
		{
			if (map == null || !map.Any())
			{
				return Enumerable.Empty<FieldMap>();
			}

			IList<string> sourceMappedArtifactsIDs = map.Select(x => x.SourceField?.FieldIdentifier).ToList();
			IFieldsClassifierRunner sourceClassifierRunner = _fieldsClassifyRunnerFactory.CreateForSourceWorkspace();
			var sourceFields = (await sourceClassifierRunner.ClassifyFieldsAsync(sourceMappedArtifactsIDs, sourceWorkspaceID).ConfigureAwait(false))
				.ToDictionary(f => f.FieldInfo.FieldIdentifier, f => f);

			IList<string> destinationMappedArtifactsIDs = map.Select(x => x.DestinationField?.FieldIdentifier).ToList();
			IFieldsClassifierRunner destinationClassifierRunner = _fieldsClassifyRunnerFactory.CreateForDestinationWorkspace();
			var destinationFields = (await destinationClassifierRunner.ClassifyFieldsAsync(destinationMappedArtifactsIDs, destinationWorkspaceID).ConfigureAwait(false))
				.ToDictionary(f => f.FieldInfo.FieldIdentifier, f => f);

			var invalidMappedFields = new List<FieldMap>();

			foreach (var fieldMap in map)
			{
				string sourceFieldFieldIdentifier = fieldMap.SourceField.FieldIdentifier ?? string.Empty;
				string destinationFieldFieldIdentifier = fieldMap.DestinationField.FieldIdentifier ?? string.Empty;

				var sourceField = sourceFields.ContainsKey(sourceFieldFieldIdentifier) ? sourceFields[sourceFieldFieldIdentifier] : null;
				var destinationField = destinationFields.ContainsKey(destinationFieldFieldIdentifier) ? destinationFields[destinationFieldFieldIdentifier] : null;

				if (!IsFieldMapValid(sourceField, destinationField, fieldMap.FieldMapType))
				{
					invalidMappedFields.Add(fieldMap);
				}
			}

			return invalidMappedFields;
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
