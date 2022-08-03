using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping
{
    public interface IFieldsRepository
    {
        Task<IEnumerable<FieldInfo>> GetAllFieldsAsync(int workspaceId, int artifactTypeId);
        Task<IEnumerable<FieldInfo>> GetFieldsByArtifactsIdAsync(IEnumerable<string> artifactIds, int workspaceId);
    }
}
