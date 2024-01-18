using System;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Executors
{
    internal interface ISyncObjectTypeManager
    {
        Task<int> EnsureObjectTypeExistsAsync(int workspaceArtifactId, Guid objectTypeGuid, ObjectTypeRequest objectTypeRequest);

        Task<QueryResult> QueryObjectTypeByNameAsync(int workspaceArtifactId, string name);

        Task<int> GetObjectTypeArtifactTypeIdAsync(int workspaceArtifactId, int objectTypeArtifactI);
    }
}
