using System;
using System.Linq;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeQueueRepository : IQueueRepository
    {
        private readonly List<JobTest> _jobQueue;

        public FakeQueueRepository(List<JobTest> jobQueue)
        {
            _jobQueue = jobQueue;
        }

        public int GetNumberOfJobsExecutingOrInQueue(int workspaceId, int integrationPointId)
        {
            return _jobQueue.Count(x =>
                x.RelatedObjectArtifactID == integrationPointId && x.WorkspaceID == workspaceId &&
                x.ScheduleRuleType == null);
        }

        public int GetNumberOfJobsExecuting(int workspaceId, int integrationPointId, long jobId, DateTime runTime)
        {
            return _jobQueue.Count(x =>
                x.RelatedObjectArtifactID == integrationPointId && x.WorkspaceID == workspaceId &&
                x.LockedByAgentID != null &&
                x.JobId != jobId);
        }

        public int GetNumberOfPendingJobs(int workspaceId, int integrationPointId)
        {
            return _jobQueue.Count(x =>
                x.RelatedObjectArtifactID == integrationPointId && x.WorkspaceID == workspaceId &&
                x.LockedByAgentID == null);
        }

        public int GetNumberOfJobsLockedByAgentForIntegrationPoint(int workspaceId, int integrationPointId)
        {
            throw new NotImplementedException();
        }
    }
}
