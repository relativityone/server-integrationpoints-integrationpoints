using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers
{
    public class JobHelper : RelativityHelperBase
    {
        public JobHelper(RelativityInstanceTest relativity) : base(relativity)
        {
        }

        public JobTest ScheduleJob(JobTest job)
        {
            Relativity.JobsInQueue.Add(job);

            return job;
        }

        public JobTest ScheduleBasicJob(WorkspaceFake workspace, DateTime? nextRunTime = null)
        {
            JobTest job = CreateBasicJob(workspace)
                .Build();

            job.NextRunTime = nextRunTime ?? DateTime.UtcNow;

            return ScheduleJob(job);
        }

        public JobTest ScheduleJobWithScheduleRule(WorkspaceFake workspace, ScheduleRuleTest rule)
        {
            JobTest job = CreateBasicJob(workspace)
                .WithScheduleRule(rule)
                .Build();

            return ScheduleJob(job);
        }

        public JobTest ScheduleIntegrationPointRun(WorkspaceFake workspace, IntegrationPointFake integrationPoint)
        {
            JobTest job = CreateBasicJob(workspace, integrationPoint)
                .Build();
            return ScheduleJob(job);
        }

        public JobTest ScheduleSyncIntegrationPointRunWithScheduleRule(WorkspaceFake workspace, IntegrationPointFake integrationPoint, ScheduleRuleTest rule = null)
        {
            JobTest job = CreateBasicJob(workspace, integrationPoint)
                .WithTaskType(TaskType.ExportService)
                .WithScheduleRule(rule)
                .Build();
            return ScheduleJob(job);
        }

        public JobTest ScheduleImportIntegrationPointRun(WorkspaceFake workspace,
            IntegrationPointFake integrationPoint, long loadFileSize, DateTime loadFileModifiedDate, int processedItemsCount)
        {
            JobTest job = CreateBasicJob(workspace, integrationPoint)
                .WithImportDetails(loadFileSize, loadFileModifiedDate, processedItemsCount)
                .Build();

            return ScheduleJob(job);
        }

        public JobTest ScheduleSyncWorkerJob(WorkspaceFake workspace, IntegrationPointFake integrationPoint, object parameters, long? rootJobId = null)
        {
            JobTest job = CreateBasicJob(workspace, integrationPoint)
                .WithJobDetails(parameters)
                .WithTaskType(TaskType.SyncWorker)
                .Build();

            job.RootJobId = rootJobId ?? JobId.Next;

            return ScheduleJob(job);
        }

        public JobTest ScheduleSyncManagerJob(WorkspaceFake workspace, IntegrationPointFake integrationPoint, object parameters, long? rootJobId = null)
        {
            JobTest job = CreateBasicJob(workspace, integrationPoint)
                .WithJobDetails(parameters)
                .WithTaskType(TaskType.SyncManager)
                .Build();

            job.RootJobId = rootJobId ?? JobId.Next;
            return ScheduleJob(job);
        }

        private JobBuilder CreateBasicJob(WorkspaceFake workspace)
        {
            IntegrationPointFake integrationPoint = workspace.Helpers.IntegrationPointHelper.CreateEmptyIntegrationPoint();
            return CreateBasicJob(workspace, integrationPoint);
        }

        private JobBuilder CreateBasicJob(WorkspaceFake workspace, IntegrationPointFake integrationPoint)
        {
            return new JobBuilder()
                .WithWorkspace(workspace)
                .WithIntegrationPoint(integrationPoint)
                .WithSubmittedBy(Relativity.TestContext.User.ArtifactId);
        }

        #region Verification

        public void VerifyJobsWithIdsAreInQueue(IEnumerable<long> jobs)
        {
            Relativity.JobsInQueue.Select(x => x.JobId).Should().Contain(jobs);
        }

        public void VerifyJobsWithIdsWereRemovedFromQueue(IEnumerable<long> jobs)
        {
            Relativity.JobsInQueue.Select(x => x.JobId).Should().NotContain(jobs);
        }

        public void VerifyJobsAreNotLockedByAgent(int agentId, IEnumerable<long> jobs)
        {
            Relativity.JobsInQueue.Where(x => jobs.Contains(x.JobId))
                .All(x => x.LockedByAgentID != agentId).Should().BeTrue();
        }

        public void VerifyScheduledJobWasReScheduled(JobTest job, DateTime expectedNextRunTime)
        {
            Relativity.JobsInQueue.Should().Contain(x =>
                x.RelatedObjectArtifactID == job.RelatedObjectArtifactID &&
                x.WorkspaceID == job.WorkspaceID &&
                x.NextRunTime.Date == expectedNextRunTime.Date);
        }

        #endregion
    }
}
