using System;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers
{
    public class JobBuilder
    {
        private readonly JobTest _job;

        public JobBuilder()
        {
            _job = new JobTest();
        }

        public JobTest Build()
        {
            return _job;
        }

        public JobBuilder WithIntegrationPoint(IntegrationPointFake integrationPoint)
        {
            _job.RelatedObjectArtifactID = integrationPoint.ArtifactId;

            return this;
        }

        public JobBuilder WithSubmittedBy(int userId)
        {
            _job.SubmittedBy = userId;

            return this;
        }

        public JobBuilder WithWorkspace(WorkspaceFake workspace)
        {
            _job.WorkspaceID = workspace.ArtifactId;

            return this;
        }

        public JobBuilder WithTaskType(TaskType taskType)
        {
            _job.TaskType = taskType.ToString();

            return this;
        }

        public JobBuilder WithScheduleRule(ScheduleRuleTest rule)
        {
            _job.ScheduleRuleType = kCura.ScheduleQueue.Core.Const._PERIODIC_SCHEDULE_RULE_TYPE;
            _job.SerializedScheduleRule = rule?.Serialize() ?? string.Empty;

            return this;
        }

        public JobBuilder WithNextUtcRunDateTime(DateTime nextUtcRunDateTime)
        {
            _job.NextRunTime = nextUtcRunDateTime;

            return this;
        }

        public JobBuilder WithImportDetails(long loadFileSize, DateTime loadFileModifiedDate, int processedItemsCount)
        {
            _job.JobDetailsHelper.BatchParameters = new LoadFileTaskParameters
            {
                Size = loadFileSize,
                LastModifiedDate = loadFileModifiedDate,
                ProcessedItemsCount = processedItemsCount
            };

            return this;
        }

        public JobBuilder WithJobDetails(object parameters)
        {
            _job.JobDetailsHelper.BatchParameters = parameters;

            return this;
        }
    }
}
