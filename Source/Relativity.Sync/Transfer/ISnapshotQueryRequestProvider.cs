using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    internal interface ISnapshotQueryRequestProvider
    {
        Task<QueryRequest> GetRequestForCurrentPipelineAsync(CancellationToken token);

        Task<QueryRequest> GetRequestWithIdentifierOnlyForCurrentPipelineAsync(CancellationToken token);

        /// <summary>
        /// Return a QueryRequest that will query for all objects that need linking
        ///
        /// Conditions: object has any field of Multi- or SingleObject type with Associated Type being the same as the
        /// transferred RDO with a set value
        /// </summary>
        /// <param name="token"></param>
        /// <returns>QueryRequest if there are any </returns>
        Task<QueryRequest> GetRequestForLinkingNonDocumentObjectsAsync(CancellationToken token);
    }
}
