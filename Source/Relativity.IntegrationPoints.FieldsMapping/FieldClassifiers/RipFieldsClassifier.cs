using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
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

        public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<FieldInfo> fields, int workspaceID)
        {
            IEnumerable<FieldClassificationResult> filteredOutFields = fields
                .Where(x => _ripFieldNamesToIgnore.Contains(x.Name))
                .Select(x => new FieldClassificationResult(x)
                {
                    ClassificationReason = "Field is populated by RIP.",
                    ClassificationLevel = ClassificationLevel.HideFromUser
                });

            return Task.FromResult(filteredOutFields);
        }
    }
}