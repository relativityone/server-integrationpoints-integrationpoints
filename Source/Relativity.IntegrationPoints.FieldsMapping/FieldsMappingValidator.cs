using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
			var invalidMappedFields = new List<FieldMap>();

			if(map == null)
			{
				return invalidMappedFields;
			}

			IList<string> sourceMappedArtifactsIDs = map.Select(x => x.SourceField.FieldIdentifier).ToList();
			IFieldsClassifierRunner sourceClassifierRunner = _fieldsClassifyRunnerFactory.CreateForSourceWorkspace();
			var sourceFields = (await sourceClassifierRunner.ClassifyFieldsAsync(sourceMappedArtifactsIDs, sourceWorkspaceID).ConfigureAwait(false))
				.ToDictionary(f => f.FieldIdentifier, f => f);

			IList<string> destinationMappedArtifactsIDs = map.Select(x => x.DestinationField.FieldIdentifier).ToList();
			IFieldsClassifierRunner destinationClassifierRunner = _fieldsClassifyRunnerFactory.CreateForDestinationWorkspace();
			var destinationFields = (await destinationClassifierRunner.ClassifyFieldsAsync(destinationMappedArtifactsIDs, destinationWorkspaceID).ConfigureAwait(false))
				.ToDictionary(f => f.FieldIdentifier, f => f);

			foreach(var fieldMap in map)
			{
				var sourceField = sourceFields[fieldMap.SourceField.FieldIdentifier];
				var destinationField = destinationFields[fieldMap.DestinationField.FieldIdentifier];
				if (!IsFieldMapValid(sourceField, destinationField))
				{
					invalidMappedFields.Add(fieldMap);
				}
			}

			return invalidMappedFields;
		}

		private bool IsFieldMapValid(FieldClassificationResult sourceField, FieldClassificationResult destinationField)
		{
			return (sourceField.ClassificationLevel == ClassificationLevel.AutoMap && destinationField.ClassificationLevel == ClassificationLevel.AutoMap)
				&& sourceField.GetFieldInfo().IsTypeCompatible(destinationField.GetFieldInfo());
		}
	}
}
