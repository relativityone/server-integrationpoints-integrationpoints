using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
	public class SystemFieldsClassifier : IFieldsClassifier
	{
		private static readonly List<string> _systemFieldNamesToIgnore = new List<string>()
		{
			"Is System Artifact",
			"System Created By",
			"System Created On",
			"System Last Modified By",
			"System Last Modified On",
			"Artifact ID"
		};

		public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<RelativityObject> fields, int workspaceID)
		{
			IEnumerable<FieldClassificationResult> filteredOutFields = fields
				.Where(x => _systemFieldNamesToIgnore.Contains(x.Name))
				.Select(x => new FieldClassificationResult()
				{
					Name = x.Name,
					ClassificationLevel = ClassificationLevel.HideFromUser,
					ClassificationReason = "System field."
				});

			return Task.FromResult(filteredOutFields);
		}
	}
}