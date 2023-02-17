using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
    public class OpenToAssociationsFieldsClassifier : IFieldsClassifier
    {
        public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<FieldInfo> fields, int workspaceID)
        {
            IEnumerable<FieldClassificationResult> fieldsWithOpenToAssociationsEnabled = fields
                .Where(x => x.OpenToAssociations.HasValue && x.OpenToAssociations.Value)
                .Select(x => new FieldClassificationResult(x)
                {
                    ClassificationLevel = ClassificationLevel.ShowToUser,
                    ClassificationReason = "This field has enabled Open To Associations."
                });

            return Task.FromResult(fieldsWithOpenToAssociationsEnabled);
        }
    }
}
