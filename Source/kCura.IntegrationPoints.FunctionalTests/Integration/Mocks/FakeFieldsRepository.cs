using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.FieldsMapping;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeFieldsRepository : IFieldsRepository
    {
        public Task<IEnumerable<DocumentFieldInfo>> GetAllDocumentFieldsAsync(int workspaceID)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<DocumentFieldInfo>> GetFieldsByArtifactsIdAsync(IEnumerable<string> artifactIDs, int workspaceID)
        {
            throw new System.NotImplementedException();
        }
    }
}
