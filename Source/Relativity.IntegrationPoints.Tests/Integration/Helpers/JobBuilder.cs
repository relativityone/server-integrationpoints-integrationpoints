using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class JobBuilder
	{
		private readonly Job _job;

		public JobBuilder()
		{
			_job = new Job();
		}

		public Job Build()
		{
			return _job;
		}

		public JobBuilder WithIntegrationPoint(IntegrationPoint integrationPoint)
		{
			_job.RelatedObjectArtifactID = integrationPoint.ArtifactId;

			return this;
		}

		public JobBuilder WithWorkspace(Workspace workspace)
		{
			_job.WorkspaceID = workspace.ArtifactId;

			return this;
		}

		public JobBuilder WithScheduleRule(ScheduleRule rule)
		{
			_job.ScheduleRuleType = kCura.ScheduleQueue.Core.Const._PERIODIC_SCHEDULE_RULE_TYPE;
			_job.SerializedScheduleRule = rule.Serialize();

			return this;
		}
	}
}
