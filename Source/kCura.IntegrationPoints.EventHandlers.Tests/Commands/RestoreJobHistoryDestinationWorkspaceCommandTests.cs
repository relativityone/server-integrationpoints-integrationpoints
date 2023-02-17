using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class RestoreJobHistoryDestinationWorkspaceCommandTests : TestBase
    {
        private IRelativityObjectManager _objectManager;
        private IFederatedInstanceManager _federatedInstanceManager;
        private IWorkspaceManager _workspaceManager;
        private RestoreJobHistoryDestinationWorkspaceCommand _command;
        private const int _WORKSPACE_ARTIFACT_ID = 623424;

        public override void SetUp()
        {
            _objectManager = Substitute.For<IRelativityObjectManager>();
            _federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
            _workspaceManager = Substitute.For<IWorkspaceManager>();

            _command = new RestoreJobHistoryDestinationWorkspaceCommand(_objectManager, new JobHistoryDestinationWorkspaceParser(_WORKSPACE_ARTIFACT_ID, _federatedInstanceManager, _workspaceManager));
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

            _objectManager.Query<JobHistory>(Arg.Any<QueryRequest>()).Returns(jobHistories);

            _federatedInstanceManager.RetrieveAll().Returns(new List<FederatedInstanceDto>
            {
                new FederatedInstanceDto
                {
                    ArtifactId = 3,
                    Name = "A"
                },
                new FederatedInstanceDto
                {
                    ArtifactId = 4,
                    Name = "C"
                }
            });

            // ACT
            _command.Execute();

            // ASSERT
            _objectManager.Received(1).Query<JobHistory>(Arg.Any<QueryRequest>());
            _objectManager.Received(1)
                .Update(Arg.Is<JobHistory>(x => x.DestinationWorkspace == "B - 1" && x.DestinationInstance == "A - 3"));
            _objectManager.Received(1)
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

            _objectManager.Query<JobHistory>(Arg.Any<QueryRequest>()).Returns(jobHistories);

            // ACT
            _command.Execute();

            // ASSERT
            _objectManager.Received(1).Query<JobHistory>(Arg.Any<QueryRequest>());
            _objectManager.Received(1)
                .Update(Arg.Is<JobHistory>(x => x.DestinationWorkspace == "B - 1" && x.DestinationInstance == FederatedInstanceManager.LocalInstance.Name));
            _objectManager.Received(1)
                .Update(Arg.Is<JobHistory>(x => x.DestinationWorkspace == "D - 2" && x.DestinationInstance == FederatedInstanceManager.LocalInstance.Name));
        }

        [Test]
        public void ItShouldHandleEmptyList()
        {
            // ARRANGE
            var jobHistories = new List<JobHistory>();

            _objectManager.Query<JobHistory>(Arg.Any<QueryRequest>()).Returns(jobHistories);

            // ACT
            _command.Execute();

            // ASSERT
            _objectManager.Received(1).Query<JobHistory>(Arg.Any<QueryRequest>());
        }

        [Test(Description = "Case for old version of RIP")]
        public void ItShouldHandleEmptyDestinationWorkspace()
        {
            // ARRANGE
            var expectedWorkspaceName = "current_workspace";

            var jobHistories = new List<JobHistory>
            {
                new JobHistory
                {
                    DestinationWorkspace = null,
                    DestinationInstance = null
                }
            };

            _objectManager.Query<JobHistory>(Arg.Any<QueryRequest>()).Returns(jobHistories);

            _workspaceManager.RetrieveWorkspace(_WORKSPACE_ARTIFACT_ID).Returns(new WorkspaceDTO
            {
                Name = expectedWorkspaceName
            });

            // ACT
            _command.Execute();

            // ASSERT
            _objectManager.Received(1).Query<JobHistory>(Arg.Any<QueryRequest>());
            _objectManager.Received(1)
                .Update(Arg.Is<JobHistory>(x => x.DestinationWorkspace == $"{expectedWorkspaceName} - {_WORKSPACE_ARTIFACT_ID}" &&
                                                x.DestinationInstance == FederatedInstanceManager.LocalInstance.Name));
        }
    }
}
