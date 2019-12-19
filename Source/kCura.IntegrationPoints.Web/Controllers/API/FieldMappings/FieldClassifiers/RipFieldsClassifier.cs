using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Web.Controllers.API.FieldMappings.FieldClassifiers
{
	public class RipFieldsClassifier : IFieldsClassifier
	{
		private static readonly List<string> _ripFieldNamesToIgnore = new List<string>()
		{
			"Relativity Source Case",
			"Relativity Source Job",
			"Relativity Destination Case",
			"Job History"
		};

		public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<RelativityObject> fields, int workspaceID)
		{
			IEnumerable<FieldClassificationResult> filteredOutFields = fields
				.Where(x => _ripFieldNamesToIgnore.Contains(x.Name))
				.Select(x => new FieldClassificationResult()
				{
					Name = x.Name,
					ClassificationReason = "Field is populated by RIP.",
					ClassificationLevel = ClassificationLevel.HideFromUser
				});

			return Task.FromResult(filteredOutFields);
		}
	}
}