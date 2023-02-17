using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
    public interface IFieldsClassifier
    {
        /// <summary>
        /// This method should only return results for fields that classifier found reason to not AutoMap
        /// </summary>
        /// <param name="fields">Fields to classify</param>
        /// <param name="workspaceID">Workspace ID</param>
        /// <returns>Classification results with ClassificationLevel other than AutoMap</returns>
        Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<FieldInfo> fields, int workspaceID);
    }
}
