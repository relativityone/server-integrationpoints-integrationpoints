using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class RestoreJobHistoryDestinationWorkspaceCommandTests : TestBase
	{
		private IRSAPIService _rsapiService;
		private IDestinationParser _destinationParser;
		private IFederatedInstanceManager _federatedInstanceManager;
		private RestoreJobHistoryDestinationWorkspaceCommand _command;

		public override void SetUp()
		{
			_rsapiService = Substitute.For<IRSAPIService>();
			_destinationParser = new DestinationParser();
			_federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
			_command = new RestoreJobHistoryDestinationWorkspaceCommand(_rsapiService, _destinationParser, _federatedInstanceManager);
		}

		[Test]
		public void ItShouldUpdateJobHistoriesWithInstanceName()
		{
			// ARRANGE
			var jobHistories = new List<JobHistory>
			{
				new JobHistory
				{
					DestinationWorkspace = "A - B - 1",
					DestinationInstance = null
				},
				new JobHistory
				{
					DestinationWorkspace = "C - D - 2",
					DestinationInstance = null
				}
			};

			_rsapiService.JobHistoryLibrary.Query(Arg.Any<Query<RDO>>()).Returns(jobHistories);

			_federatedInstanceManager.RetrieveFederatedInstanceByName("A").Returns(new FederatedInstanceDto
			{
				ArtifactId = 3
			});
			_federatedInstanceManager.RetrieveFederatedInstanceByName("C").Returns(new FederatedInstanceDto
			{
				ArtifactId = 4
			});

			// ACT
			_command.Execute();

			// ASSERT
			_rsapiService.JobHistoryLibrary.Received(1).Query(Arg.Any<Query<RDO>>());
			_rsapiService.JobHistoryLibrary.Received(1)
				.Update(Arg.Is<JobHistory>(x => x.DestinationWorkspace == "B - 1" && x.DestinationInstance == "A - 3"));
			_rsapiService.JobHistoryLibrary.Received(1)
				.Update(Arg.Is<JobHistory>(x => x.DestinationWorkspace == "D - 2" && x.DestinationInstance == "C - 4"));
		}

		[Test]
		public void ItShouldUpdateJobHistoriesWithoutInstanceName()
		{
			// ARRANGE
			var jobHistories = new List<JobHistory>
			{
				new JobHistory
				{
					DestinationWorkspace = "B - 1",
					DestinationInstance = null
				},
				new JobHistory
				{
					DestinationWorkspace = "D - 2",
					DestinationInstance = null
				}
			};

			_rsapiService.JobHistoryLibrary.Query(Arg.Any<Query<RDO>>()).Returns(jobHistories);

			// ACT
			_command.Execute();

			// ASSERT
			_rsapiService.JobHistoryLibrary.Received(1).Query(Arg.Any<Query<RDO>>());
			_rsapiService.JobHistoryLibrary.Received(1)
				.Update(Arg.Is<JobHistory>(x => x.DestinationWorkspace == "B - 1" && x.DestinationInstance == FederatedInstanceManager.LocalInstance.Name));
			_rsapiService.JobHistoryLibrary.Received(1)
				.Update(Arg.Is<JobHistory>(x => x.DestinationWorkspace == "D - 2" && x.DestinationInstance == FederatedInstanceManager.LocalInstance.Name));
		}

		[Test]
		public void ItShouldHandleEmptyList()
		{
			// ARRANGE
			var jobHistories = new List<JobHistory>();

			_rsapiService.JobHistoryLibrary.Query(Arg.Any<Query<RDO>>()).Returns(jobHistories);

			// ACT
			_command.Execute();

			// ASSERT
			_rsapiService.JobHistoryLibrary.Received(1).Query(Arg.Any<Query<RDO>>());
		}
	}
}