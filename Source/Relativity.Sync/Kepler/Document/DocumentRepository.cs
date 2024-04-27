using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Kepler.Extensions;
using Relativity.Sync.Kepler.SyncBatch;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Kepler.Document
{
    internal class DocumentRepository : IDocumentRepository
    {
        private readonly IProxyFactoryDocument _serviceFactory;

        public DocumentRepository(IProxyFactoryDocument serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<List<int>> GetErroredDocumentsByBatchAsync(SyncBatchDto batch, Identity identity)
        {
            QueryRequest request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
                Condition = $"'ErroredDocuments' SUBQUERY ('ErroredDocuments' INTERSECTS MULTIOBJECT [{batch.ArtifactId}])"
            };
            IObjectManager objectManager = await _serviceFactory.CreateProxyDocumentAsync<IObjectManager>(identity).ConfigureAwait(false);
            List<RelativityObjectSlim> exportResult = await objectManager.QueryUsingExportAsync(batch.WorkspaceArtifactId, request).ConfigureAwait(false);

            return exportResult.Select(x => x.ArtifactID).ToList();
        }
    }
}
