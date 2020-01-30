using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
	public class OpenToAssociationsFieldsClassifier : IFieldsClassifier
	{
		public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<RelativityObject> fields, int workspaceID)
		{
			IEnumerable<FieldClassificationResult> fieldsWithOpenToAssociationsEnabled = fields
				.Where(x => x.FieldValues
					.Exists(fieldValuePair => fieldValuePair
												  .Field.Name.Equals("Open To Associations") &&
											  fieldValuePair.Value is bool value &&
											  value == true))
				.Select(x => new FieldClassificationResult()
				{
					Name = x.Name,
					FieldIdentifier = x.ArtifactID.ToString(),
					ClassificationLevel = ClassificationLevel.ShowToUser,
					ClassificationReason = "This field has enabled Open To Associations."
				});

			return Task.FromResult(fieldsWithOpenToAssociationsEnabled);
		}
	}
}