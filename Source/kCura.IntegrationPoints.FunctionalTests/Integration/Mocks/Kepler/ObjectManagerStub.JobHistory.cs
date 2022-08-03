using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using Match = System.Text.RegularExpressions.Match;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
        private void SetupJobHistory()
        {
            bool IsIntegrationPointCondition(string condition, out int integrationPointId)
            {
                Match match = Regex.Match(condition,
                   $@"'Integration Point' INTERSECTS MULTIOBJECT \[(\d+)\]");

                if (match.Success && int.TryParse(match.Groups[1].Value, out int extractedId))
                {
                    integrationPointId = extractedId;
                    return true;
                }

                integrationPointId = -1;
                return false;
            }
            
            bool IsArtifactIdCondition(string condition, out int artifactId)
            {
                Match match = Regex.Match(condition,
                    $@"'Artifact ID' == '(\d+)'");

                if (match.Success && int.TryParse(match.Groups[1].Value, out int extractedId))
                {
                    artifactId = extractedId;
                    return true;
                }

                artifactId = -1;
                return false;
            }

            bool IsBatchInstanceCondition(string condition, out string batchInstance)
            {
                Match match = Regex.Match(condition,
                    $"'{JobHistoryFields.BatchInstance}' == '(.*)'");

                if (match.Success)
                {
                    batchInstance = match.Groups[1].Value;
                    return true;
                }

                batchInstance = null;
                return false;
            }

            IList<JobHistoryTest> JobHistoryFilter(QueryRequest request, IList<JobHistoryTest> list)
            {
                if (IsIntegrationPointCondition(request.Condition, out int integrationPointId))
                {
                    return list.Where(x => x.IntegrationPoint.Contains(integrationPointId)).ToList();
                }

                if (IsBatchInstanceCondition(request.Condition, out string batchInstance))
                {
                    return list.Where(x => x.BatchInstance == batchInstance).ToList();
                }

                if (IsArtifactIdCondition(request.Condition, out int artifactId))
                {
                    return list.Where(x => x.ArtifactId == artifactId).ToList();
                }

                return new List<JobHistoryTest>();
            }
            
            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), 
                    It.Is<QueryRequest>(r => IsJobHistoryQueryRequest(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
                    {
                        QueryResult result = GetRelativityObjectsForRequest(x => x.JobHistory, JobHistoryFilter, workspaceId, request, length);
                        return Task.FromResult(result);
                    }
                );

            Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsJobHistoryQueryRequest(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
                {
                    QueryResultSlim result = GetQuerySlimsForRequest(x=>x.JobHistory, JobHistoryFilter, workspaceId, request, length);
                    return Task.FromResult(result);
                });
        }

        private bool IsJobHistoryQueryRequest(QueryRequest x)
        {
            return x.ObjectType.Guid.HasValue &&
                   x.ObjectType.Guid.Value.Equals(ObjectTypeGuids.JobHistoryGuid);
        }
    }
}