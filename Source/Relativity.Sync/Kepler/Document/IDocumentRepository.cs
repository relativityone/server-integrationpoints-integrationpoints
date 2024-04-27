using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Kepler.SyncBatch;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Kepler.Document
{
    internal interface IDocumentRepository
    {
        Task<List<int>> GetErroredDocumentsByBatchAsync(SyncBatchDto batch, Identity identity);
    }
}
