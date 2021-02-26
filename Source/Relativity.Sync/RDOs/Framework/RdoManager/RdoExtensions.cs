using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs.Framework
{
    internal static class RdoExtensions
    {
        public static async Task<string> GetObjectNameAsync(this IObjectManager objectManager, int workspaceId, int artifactId,
            Guid objectTypeGuid)
        {
            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = objectTypeGuid
                },
                Condition = $"'ArtifactId' == `{artifactId}`",
                IncludeNameInQueryResult = true
            };

            var result = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);
            return result.Objects.First().Name;
        }
    }
}