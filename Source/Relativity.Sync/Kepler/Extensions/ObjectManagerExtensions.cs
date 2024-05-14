using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Kepler.Extensions
{
    internal static class ObjectManagerExtensions
    {
        public const int _EXPORT_API_DEFAULT_BATCH_SIZE = 1000;

        public static async Task<List<RelativityObjectSlim>> QueryUsingExportAsync(this IObjectManager objectManager, int workspaceId, QueryRequest request)
        {
            ExportInitializationResults export = await objectManager.InitializeExportAsync(workspaceId, request, 0).ConfigureAwait(false);

            List<RelativityObjectSlim> result = new List<RelativityObjectSlim>();

            RelativityObjectSlim[] exportBatch;
            do
            {
                exportBatch = await objectManager.RetrieveNextResultsBlockFromExportAsync(
                        workspaceId, export.RunID, _EXPORT_API_DEFAULT_BATCH_SIZE)
                    .ConfigureAwait(false);

                if (exportBatch is null)
                {
                    break;
                }

                result.AddRange(exportBatch);
            }
            while (exportBatch.Any());

            return result;
        }
    }
}
