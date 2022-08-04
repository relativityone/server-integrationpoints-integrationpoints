using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class JobHistoryDestinationWorkspaceParserTests : TestBase
    {
        private IWorkspaceManager _workspaceManager;
        private const int _LOCAL_WORKSPACE_ID = 554413;

        public override void SetUp()
        {
            _workspaceManager = Substitute.For<IWorkspaceManager>();
        }

        [Test]
        public void DestinationParser_Parse_InputNull_ReturnsLocalParentWorkspaceWithName()
        {
            IFederatedInstanceManager instanceManager = Substitute.For<IFederatedInstanceManager>();
            _workspaceManager.RetrieveWorkspace(_LOCAL_WORKSPACE_ID).Returns(new WorkspaceDTO {ArtifactId = _LOCAL_WORKSPACE_ID, Name = "Local Workspace"});
            instanceManager.RetrieveAll().Returns(new List<FederatedInstanceDto>());

            var parser = new JobHistoryDestinationWorkspaceParser(_LOCAL_WORKSPACE_ID, instanceManager, _workspaceManager);

            DestinationWorkspaceElementsParsingResult result = parser.Parse(null);

            Assert.AreEqual("Local Workspace - 554413", result.WorkspaceName);
            Assert.AreEqual("This Instance", result.InstanceName);
        }

        [Test]
        public void DestinationParser_Parse_InputDoesntEndWithId_ReturnsEmptyResult()
        {
            IFederatedInstanceManager instanceManager = Substitute.For<IFederatedInstanceManager>();
            instanceManager.RetrieveAll().Returns(new List<FederatedInstanceDto>());

            var parser = new JobHistoryDestinationWorkspaceParser(_LOCAL_WORKSPACE_ID, instanceManager, _workspaceManager);

            DestinationWorkspaceElementsParsingResult result = parser.Parse("Whatever - is-written - here. At the end - there is no - number");

            Assert.IsNull(result.InstanceName);
            Assert.IsNull(result.WorkspaceName);
        }

        [TestCase("Workspace-7890")]
        [TestCase("Workspace - 7890")]
        [TestCase("Workspace - with - dashes - -7890")]
        [TestCase("Workspace - 54353 - with - dashes and - numbers -- 4583952 - -7890")]
        public void DestinationParser_Parse_NoInstanceDataInProperty_ReturnsProperResult(string input)
        {
            IFederatedInstanceManager instanceManager = Substitute.For<IFederatedInstanceManager>();
            instanceManager.RetrieveAll().Returns(new List<FederatedInstanceDto>
            {
                new FederatedInstanceDto {ArtifactId = 65432, Name = "Other name 1"},
                new FederatedInstanceDto { ArtifactId = 12345, Name = "Federated instance" },
                new FederatedInstanceDto {ArtifactId = 98765, Name = "Other name 2"}
            });
            var parser = new JobHistoryDestinationWorkspaceParser(_LOCAL_WORKSPACE_ID, instanceManager, _workspaceManager);

            DestinationWorkspaceElementsParsingResult result = parser.Parse(input);

            Assert.AreEqual(result.InstanceName, "This Instance");
            Assert.AreEqual(result.WorkspaceName, input);
        }

        [TestCase("Federated instance - Workspace-7890", "Federated instance", 7890)]
        [TestCase("Federated instance - Workspace - with - dashes - -78904", "Federated instance", 78904)]
        [TestCase("Federated instance - Workspace - 54353 - with - dashes and - numbers -- 4583952 - -78905", "Federated instance", 78905)]
        [TestCase("Federated instance - with - dashes - and - Workspace - 54353 - with - dashes and - numbers -- 4583952 - -78906", "Federated instance - with - dashes - and", 78906)]
        public void DestinationParser_Parse_InstanceDataInProperty_ReturnsProperResult(string input, string instanceName, int workspaceId)
        {
            IFederatedInstanceManager instanceManager = Substitute.For<IFederatedInstanceManager>();
            instanceManager.RetrieveAll().Returns(new List<FederatedInstanceDto>
            {
                new FederatedInstanceDto {ArtifactId = 65432, Name = "Other name 1"},
                new FederatedInstanceDto { ArtifactId = 12345, Name = instanceName },
                new FederatedInstanceDto {ArtifactId = 98765, Name = "Other name 2"},
            });
            var parser = new JobHistoryDestinationWorkspaceParser(_LOCAL_WORKSPACE_ID, instanceManager, _workspaceManager);

            DestinationWorkspaceElementsParsingResult result = parser.Parse(input);

            Assert.AreEqual(result.InstanceName, $"{instanceName} - 12345");
            Assert.IsTrue(result.WorkspaceName.EndsWith(workspaceId.ToString()));
        }

        [TestCase("This instance - Workspace - 78902", "Workspace - 78902")]
        [TestCase("This instance - Workspace with-dashes - - and - 6895 numbers - - 78902",
            "Workspace with-dashes - - and - 6895 numbers - - 78902")]
        public void DestinationParser_Parse_ThisInstanceNameSet_ReturnsProperResult(string input, string workspacePart)
        {
            IFederatedInstanceManager instanceManager = Substitute.For<IFederatedInstanceManager>();
            instanceManager.RetrieveAll().Returns(new List<FederatedInstanceDto>());
            var parser = new JobHistoryDestinationWorkspaceParser(_LOCAL_WORKSPACE_ID, instanceManager, _workspaceManager);

            DestinationWorkspaceElementsParsingResult result = parser.Parse(input);

            Assert.AreEqual(result.InstanceName, "This Instance");
            Assert.AreEqual(result.WorkspaceName, workspacePart);
        }
    }
}