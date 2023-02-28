using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class KeplerWorkspacesRepositoryTests
    {
        private IHelper _helper;
        private IServicesMgr _servicesMgr;
        private IWorkspaceManager _workspaceManagerProxy;
        private IRelativityObjectManager _relativityObjectManager;

        [SetUp]
        public void SetUp()
        {
            _helper = Substitute.For<IHelper>();
            _servicesMgr = Substitute.For<IServicesMgr>();
            _workspaceManagerProxy = Substitute.For<IWorkspaceManager>();
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();
        }

        [Test]
        public void ItShouldRetrieveAllActive()
        {
            // Arrange
            const int workspaceId = 0;
            const string workspaceName = "New Workspace";
            var workspaces = new List<WorkspaceRef>();
            workspaces.Add(new WorkspaceRef() { ArtifactID = workspaceId, Name = workspaceName });
            _workspaceManagerProxy.RetrieveAllActive().Returns(workspaces);
            _servicesMgr.CreateProxy<IWorkspaceManager>(ExecutionIdentity.CurrentUser).Returns(_workspaceManagerProxy);
            var repository = new KeplerWorkspaceRepository(_helper, _servicesMgr, _relativityObjectManager);

            // Act
            List<WorkspaceDTO> resultList = repository.RetrieveAllActive().ToList();

            // Assert
            Assert.AreEqual(resultList.Count, 1);
            Assert.AreEqual(resultList.First().Name, workspaceName);
            Assert.AreEqual(resultList.First().ArtifactId, workspaceId);
        }
    }
}
