using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
    public class ObjectFieldsClassifier : IFieldsClassifier
    {
        private const string ApiDoesNotSupportAllObjectTypes = "API does not support all object types.";

        public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<FieldInfo> fields, int workspaceID)
        {
            IEnumerable<FieldClassificationResult> objectFields = fields
                .Where(x => x.Type == FieldTypeName.SINGLE_OBJECT || x.Type == FieldTypeName.MULTIPLE_OBJECT)
                .Select(x => new FieldClassificationResult(x)
                {
                    ClassificationReason = ApiDoesNotSupportAllObjectTypes,
                    ClassificationLevel = ClassificationLevel.ShowToUser
                });

            return Task.FromResult(objectFields);
        }
    }
}