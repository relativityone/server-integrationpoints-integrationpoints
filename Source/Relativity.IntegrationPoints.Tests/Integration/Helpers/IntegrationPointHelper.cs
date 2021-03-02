using System;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class IntegrationPointHelper : HelperBase
	{
		public IntegrationPointHelper(InMemoryDatabase database, ProxyMock proxyMock) : base(database, proxyMock)
		{
		}

		public IntegrationPoint CreateEmptyIntegrationPoint(Workspace workspace)
		{
			var integrationPoint = new IntegrationPoint
			{
				WorkspaceId = workspace.ArtifactId
			};

			Database.IntegrationPoints.Add(integrationPoint);

			return integrationPoint;
		}

		public Job ScheduleIntegrationPointJob(IntegrationPoint integrationPoint)
		{
			Job job = new Job
			{
				JobId = JobId.Next,
				AgentTypeID = Const.Agent._INTEGRATION_POINTS_AGENT_TYPE_ID,
				NextRunTime = DateTime.Now,
				RelatedObjectArtifactID = integrationPoint.ArtifactId,
				WorkspaceID = integrationPoint.WorkspaceId
			};

			Database.JobsInQueue.Add(job);

			return job;
		}
	}
}
