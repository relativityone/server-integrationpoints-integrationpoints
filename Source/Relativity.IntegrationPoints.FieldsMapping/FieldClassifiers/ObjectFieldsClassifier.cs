using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
	public class ObjectFieldsClassifier : IFieldsClassifier
	{
		private const string ApiDoesNotSupportAllObjectTypes = "API does not support all object types.";

		public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<RelativityObject> fields, int workspaceID)
		{
			IEnumerable<FieldClassificationResult> objectFields = fields
				.Where(x => x.FieldValues.Exists(fieldValuePair =>
								(fieldValuePair.Field.Name == "Field Type" &&
								 (fieldValuePair.Value.ToString() == "Single Object" ||
								  fieldValuePair.Value.ToString() == "Multiple Object"))))
				.Select(x => new FieldClassificationResult()
				{
					Name = x.Name,
					ClassificationReason = ApiDoesNotSupportAllObjectTypes,
					ClassificationLevel = ClassificationLevel.ShowToUser
				});

			return Task.FromResult(objectFields);
		}
	}
}