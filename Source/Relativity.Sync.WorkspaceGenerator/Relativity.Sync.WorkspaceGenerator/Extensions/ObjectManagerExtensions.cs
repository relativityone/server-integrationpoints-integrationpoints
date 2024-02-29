using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.WorkspaceGenerator.Extensions
{
    internal static class ObjectManagerExtensions
    {
        public static async Task<IEnumerable<QueryResult>> QueryAllAsync(
            this IObjectManager objectManager,
            int workspaceId,
            QueryRequest request,
            int startingIndex = 0)
        {
            const int batchSize = 1000;

            int currentIndex = startingIndex;
            QueryResult initialResult = await objectManager.QueryAsync(workspaceId,
                request,
                currentIndex,
                batchSize).ConfigureAwait(false);

            var results = new List<QueryResult> { initialResult };
            int readSoFar = initialResult.ResultCount;
            currentIndex += initialResult.ResultCount;
            int totalCount = initialResult.TotalCount;

            while (readSoFar < totalCount)
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId,
                    request,
                    currentIndex,
                    batchSize).ConfigureAwait(false);

                readSoFar += result.ResultCount;
                currentIndex += result.ResultCount;
                results.Add(result);
            }

            return results;
        }
    }
}
