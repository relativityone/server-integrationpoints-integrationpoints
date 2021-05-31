using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Choice;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;
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
                    @"[(]'Integration Point' INTERSECTS MULTIOBJECT \[(\d+)\]");

                if (match.Success && int.TryParse(match.Groups[1].Value, out int extractedId))
                {
                    integrationPointId = extractedId;
                    return true;
                }

                integrationPointId = -1;
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

            Mock.Setup(x => x.UpdateAsync(It.IsAny<int>(),
                    It.Is<UpdateRequest>(u => u.FieldValues.Any(f => JobHistoryTest.Guids.Contains(f.Field.Guid.Value)))))
                .Returns((int workspaceId, UpdateRequest request) =>
                {
                    JobHistoryTest jobHistory = _relativity.Workspaces.Single(x => x.ArtifactId == workspaceId)
                        .JobHistory.Single(x => x.ArtifactId == request.Object.ArtifactID);

                    foreach (var field in request.FieldValues)
                    {
                        jobHistory.Values[field.Field.Guid.Value] = field.Value;
                    }

                    return Task.FromResult(new UpdateResult());
                });
	            
            Mock.Setup(x => x.UpdateAsync(It.IsAny<int>(),   
                    It.Is<UpdateRequest>(r => IsJobHistoryUpdateJobStatusRequest(r))))
	            .Returns((int workspaceId, UpdateRequest request) =>
	            {
		            JobHistoryTest jobHistory = _relativity
			            .Workspaces
			            .Single(x => x.ArtifactId == workspaceId)
			            .JobHistory
			            .Single(x => x.ArtifactId == request.Object.ArtifactID);

		            FieldRefValuePair jobStatusFieldValuePair = request.FieldValues.Single(x => x.Field.Guid == JobHistoryFieldGuids.JobStatusGuid);
		            Relativity.Services.Objects.DataContracts.ChoiceRef jobStatus = jobStatusFieldValuePair.Value as Relativity.Services.Objects.DataContracts.ChoiceRef;
                    
		            jobHistory.JobStatus = new ChoiceRef(new List<Guid>() { jobStatus.Guid.Value });

		            UpdateResult result = new UpdateResult()
		            {
                        EventHandlerStatuses = new List<EventHandlerStatus>()
		            };
		            return Task.FromResult(result);
	            });
        }

        private bool IsJobHistoryQueryRequest(QueryRequest x)
        {
	        return x.ObjectType.Guid.HasValue &&
	               x.ObjectType.Guid.Value.Equals(ObjectTypeGuids.JobHistoryGuid);
        }

        private bool IsJobHistoryUpdateJobStatusRequest(UpdateRequest request)
        {
	        bool isJobHistoryArtifactId = _relativity.Workspaces.Any(x => x.JobHistory.Any(y => y.ArtifactId == request.Object.ArtifactID));
	        bool hasJobStatusField = request.FieldValues.SingleOrDefault(x => x.Field.Guid == JobHistoryFieldGuids.JobStatusGuid)?.Value != null;

            return isJobHistoryArtifactId && hasJobStatusField;
        }
    }
}