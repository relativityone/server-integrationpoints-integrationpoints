using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Common.Extensions
{
    internal static class IObjectManagerExtensions
    {
        public static async Task<RelativityObjectSlim> QuerySingleSlimAsync(this IObjectManager objectManager, int workspaceID, QueryRequest request)
        {
            QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceID, request, 0, 1).ConfigureAwait(false);

            return result.Objects.FirstOrDefault();
        }
    }
}
