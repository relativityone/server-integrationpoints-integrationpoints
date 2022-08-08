using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping
{
    public interface IFieldsClassifierRunner
    {
        Task<IList<FieldClassificationResult>> GetFilteredFieldsAsync(int workspaceID, int artifactTypeId);
        Task<IEnumerable<FieldClassificationResult>> ClassifyFieldsAsync(ICollection<FieldInfo> fields, int workspaceID);
        Task<IEnumerable<FieldClassificationResult>> ClassifyFieldsAsync(ICollection<string> artifactIDs, int workspaceID);
    }
}