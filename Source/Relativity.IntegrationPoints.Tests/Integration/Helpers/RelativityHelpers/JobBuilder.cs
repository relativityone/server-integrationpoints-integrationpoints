using kCura.IntegrationPoints.Core.Contracts.Import;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;

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

		public JobBuilder WithIntegrationPoint(IntegrationPointTest integrationPoint)
		{
			_job.RelatedObjectArtifactID = integrationPoint.ArtifactId;

			return this;
		}

		public JobBuilder WithSubmittedBy(int userId)
		{
			_job.SubmittedBy = userId;

			return this;
		}

		public JobBuilder WithWorkspace(WorkspaceTest workspace)
		{
			_job.WorkspaceID = workspace.ArtifactId;

			return this;
		}

		public JobBuilder WithScheduleRule(ScheduleRuleTest rule)
		{
			_job.ScheduleRuleType = kCura.ScheduleQueue.Core.Const._PERIODIC_SCHEDULE_RULE_TYPE;
			_job.SerializedScheduleRule = rule.Serialize();

			return this;
		}

		public JobBuilder WithImportDetails(long loadFileSize, DateTime loadFileModifiedDate, int processedItemsCount)
		{
			var loadFileParameters = new LoadFileTaskParameters
			{
				Size = loadFileSize,
				LastModifiedDate = loadFileModifiedDate,
				ProcessedItemsCount = processedItemsCount
			};

			_job.JobDetailsHelper.BatchParameters = loadFileParameters;

			return this;
		}
	}
}
