using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class UpdateDestinationWorkspaceEntriesCommandTests : TestBase
	{
		private UpdateDestinationWorkspaceEntriesCommand _command;
		private IRSAPIService _rsapiService;
		private IDestinationWorkspaceRepository _destinationWorkspaceRepository;

		public override void SetUp()
		{
			_rsapiService = Substitute.For<IRSAPIService>();
			_destinationWorkspaceRepository = Substitute.For<IDestinationWorkspaceRepository>();
			_command = new UpdateDestinationWorkspaceEntriesCommand(_rsapiService, _destinationWorkspaceRepository);
		}

		[Test]
		public void GoldWorkflow()
		{
			// ASSERT
			var federatedInstanceDto = new FederatedInstanceDto
			{
				Name = "This Instance"
			};

			var entriesToUpdate = new List<DestinationWorkspace>
			{
				new DestinationWorkspace(),
				new DestinationWorkspace()
			};
			_rsapiService.DestinationWorkspaceLibrary.Query(Arg.Any<Query<RDO>>()).Returns(entriesToUpdate);

			// ACT
			_command.Execute();

			// ASSERT
			_rsapiService.DestinationWorkspaceLibrary.Received(1).Query(Arg.Any<Query<RDO>>());

			foreach (var destinationWorkspace in entriesToUpdate)
			{
				_destinationWorkspaceRepository.Received(1).Update(Arg.Is<DestinationWorkspace>(x => x == destinationWorkspace && x.DestinationInstanceName == federatedInstanceDto.Name));
			}
		}
	}
}