using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.Tab;
using Relativity.Services.Interfaces.Tab.Models;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture]
    public class TabRepositoryTests
    {
        private Mock<ITabManager> _tabManager;
        private const int _WORKSPACE_ID = 1111;

        private TabRepository _sut;

        [SetUp]
        public void SetUp()
        {
            _tabManager = new Mock<ITabManager>();
            Mock<IServicesMgr> servicesMgr = new Mock<IServicesMgr>();
            servicesMgr.Setup(x => x.CreateProxy<ITabManager>(It.IsAny<ExecutionIdentity>())).Returns(_tabManager.Object);

            _sut = new TabRepository(servicesMgr.Object, _WORKSPACE_ID);
        }

        [Test]
        public void RetrieveTabArtifactId_ShouldReturnTabArtifactId_WhenTabExists()
        {
            // Arrange
            const int objectTypeArtifactID = 2222;
            const int tabArtifactId = 3333;
            const string tabName = "My Tab";
            NavigationTabResponse tabResponse = new NavigationTabResponse()
            {
                ArtifactID = tabArtifactId,
                ObjectTypeIdentifier = new Securable<ObjectTypeIdentifier>(new ObjectTypeIdentifier()
                {
                    ArtifactTypeID = objectTypeArtifactID
                }),
                Name = tabName
            };
            _tabManager.Setup(x => x.GetAllNavigationTabs(_WORKSPACE_ID)).ReturnsAsync(new List<NavigationTabResponse>(){tabResponse});

            // Act
            int? tabId = _sut.RetrieveTabArtifactId(objectTypeArtifactID, tabName);

            // Assert
            tabId.Should().Be(tabArtifactId);
        }

        [Test]
        public void RetrieveTabArtifactId_ShouldReturnNull_WhenTabDoesntExist()
        {
            // Arrange
            _tabManager.Setup(x => x.GetAllNavigationTabs(_WORKSPACE_ID)).ReturnsAsync(new List<NavigationTabResponse>());

            // Act
            int? tabId = _sut.RetrieveTabArtifactId(5555, "some tab");

            // Assert
            tabId.Should().BeNull();
        }

        [Test]
        public void RetrieveTabArtifactId_ShouldReturnNull_WhenTabManagerReturnsNull()
        {
            // Arrange
            _tabManager.Setup(x => x.GetAllNavigationTabs(_WORKSPACE_ID)).ReturnsAsync(null);

            // Act
            int? tabId = _sut.RetrieveTabArtifactId(5555, "some tab");

            // Assert
            tabId.Should().BeNull();
        }

        [Test]
        public void Delete_ShouldDeleteTab()
        {
            // Arrange
            const int tabId = 7777;

            // Act
            _sut.Delete(tabId);

            // Assert
            _tabManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, tabId), Times.Once);
        }
    }
}