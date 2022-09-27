using System.Data;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using Moq;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
    public class ScheduleQueueInitializationTests : TestsBase
    {
        public override void SetUp()
        {
            base.SetUp();

            DataTable result = new DataTable
            {
                Columns = { new DataColumn() }
            };

            Helper.DbContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(result);
        }

        [IdentifiedTest("544F7D10-F5FA-4848-AA50-2543FCB22A03")]
        public void ScheduleQueueTable_ShouldBeCreatedOnce_WhenAgentIsRunning()
        {
            // Arrange
            AgentTest agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();
            QueueQueryManagerMock queryManagerMock = (QueueQueryManagerMock)Container.Resolve<IQueueQueryManager>();
            FakeAgent sut = new FakeAgent(Container, agent,
                Container.Resolve<IAgentHelper>(),
                queryManager: queryManagerMock,
                kubernetesMode: Container.Resolve<IKubernetesMode>());

            // Act
            sut.Execute();

            // Assert
            queryManagerMock.ShouldCreateQueueTable();
        }

        [IdentifiedTest("C4771EA2-4E49-461C-A57C-5EA8E8C0E4D2")]
        public void Agent_ShouldRemoveJobs_WhenAreNotLockedAndCorrespondingWorkspaceDoesNotExist()
        {
            // Arrange
            JobTest jobWithoutWorkspace = FakeRelativityInstance.Helpers.JobHelper.ScheduleJob(new JobTest()
            {
                WorkspaceID = ArtifactProvider.NextId()
            });

            FakeAgent sut = FakeAgent.CreateWithEmptyProcessJob(FakeRelativityInstance, Container);

            // Act
            sut.Enabled = false;

            sut.Execute();

            // Assert
            sut.VerifyJobsWereNotProcessed(new [] {jobWithoutWorkspace.JobId});

            FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(new []{jobWithoutWorkspace.JobId});
        }
    }
}
