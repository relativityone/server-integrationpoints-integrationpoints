using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.Commands;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class UpdateDestinationWorkspaceEntriesCommandTests : TestBase
    {
        private UpdateDestinationWorkspaceEntriesCommand _command;
        private IRelativityObjectManager _relativityObjectManager;
        private IDestinationWorkspaceRepository _destinationWorkspaceRepository;

        public override void SetUp()
        {
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();
            _destinationWorkspaceRepository = Substitute.For<IDestinationWorkspaceRepository>();
            _command = new UpdateDestinationWorkspaceEntriesCommand(_relativityObjectManager, _destinationWorkspaceRepository);
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
            _relativityObjectManager.Query<DestinationWorkspace>(Arg.Any<QueryRequest>()).Returns(entriesToUpdate);

            // ACT
            _command.Execute();

            // ASSERT
            _relativityObjectManager.Received(1).Query<DestinationWorkspace>(Arg.Any<QueryRequest>());

            foreach (var destinationWorkspace in entriesToUpdate)
            {
                _destinationWorkspaceRepository.Received(1).Update(Arg.Is<DestinationWorkspace>(x => x == destinationWorkspace && x.DestinationInstanceName == federatedInstanceDto.Name));
            }
        }
    }
}
