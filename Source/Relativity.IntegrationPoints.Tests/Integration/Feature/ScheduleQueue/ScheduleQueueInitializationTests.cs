using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Feature.ScheduleQueue
{
	[TestFixture]
	public class ScheduleQueueInitializationTests : TestsBase
	{
		[Test]
		public void ScheduleQueueTable_ShouldBeCreatedOnce_WhenAgentIsRunning()
		{
			// Arrange
			Agent agent = SetUpAgent();
			
			QueryManagerMock queryManagerMock = (QueryManagerMock)Container.Resolve<IQueryManager>();
			
			ScheduleTestAgent sut = new ScheduleTestAgent(agent.AgentGuid,
				Container.Resolve<IAgentHelper>(),
				queryManager: queryManagerMock);

			// Act
			sut.Execute();

			// Assert
			queryManagerMock.ShouldCreateQueueTable();
		}

		private Agent SetUpAgent()
		{
			Agent agent = Agent.CreateIntegrationPointsAgent();

			Database.Agents.Add(agent);

			return agent;
		}
	}
}
