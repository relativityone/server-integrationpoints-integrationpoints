using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Tests.System.Core.Extensions
{
    public static class ObjectManagerExtensions
    {
        private static readonly Guid JobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
        private static readonly Guid ErrorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
        private static readonly Guid StackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");

        public static async Task<IEnumerable<RelativityObject>> GetAllJobHistoryErrors(this IObjectManager objectManager, int workspaceId,
            int jobHistoryId)
        {
            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { Guid = JobHistoryErrorObject },
                Condition = $"'Job History' == {jobHistoryId}",
                Fields = new List<FieldRef>
                {
                    new FieldRef { Guid = ErrorMessageField },
                    new FieldRef { Guid = StackTraceField }
                }
            };

            IEnumerable<QueryResult> results = await objectManager.QueryAllAsync(workspaceId, request).ConfigureAwait(false);

            return results.SelectMany(x => x.Objects);
        }

        public static async Task<IEnumerable<QueryResult>> QueryAllAsync(
            this IObjectManager objectManager,
            int workspaceId,
            QueryRequest request,
            int startingIndex = 0)
        {
            const int batchSize = 1000;

            int currentIndex = startingIndex;
            QueryResult initialResult = await objectManager.QueryAsync(
                workspaceId,
                request,
                currentIndex,
                batchSize).ConfigureAwait(false);

            var results = new List<QueryResult> { initialResult };
            int readSoFar = initialResult.ResultCount;
            int totalCount = initialResult.TotalCount;

            while (readSoFar < totalCount)
            {
                QueryResult result = await objectManager.QueryAsync(
                    workspaceId,
                    request,
                    currentIndex,
                    batchSize).ConfigureAwait(false);

                readSoFar += result.ResultCount;
                results.Add(result);
            }

            return results;
        }

        public static async Task<string> AggregateJobHistoryErrorMessagesAsync(this IObjectManager objectManager, int workspaceId, int jobHistoryId)
        {
            IEnumerable<RelativityObject> jobHistoryErrors =
                await objectManager.GetAllJobHistoryErrors(workspaceId, jobHistoryId)
                    .ConfigureAwait(false);

            var sb = new StringBuilder();

            foreach (RelativityObject err in jobHistoryErrors)
            {
                sb.AppendLine($"Item level error: {err[ErrorMessageField].Value}")
                    .AppendLine((string)err[StackTraceField].Value)
                    .AppendLine();
            }

            return sb.ToString();
        }
    }
}
