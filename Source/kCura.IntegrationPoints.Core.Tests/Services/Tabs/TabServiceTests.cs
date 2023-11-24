using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Services.Tabs;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.Tab;
using Relativity.Services.Interfaces.Tab.Models;

namespace kCura.IntegrationPoints.Core.Tests.Services.Tabs
{
    [TestFixture]
    public class TabServiceTests
    {
        private Mock<ITabManager> _tabManagerMock;
        private Mock<IServicesMgr> _servicesMgrFake;
        private TabService _sut;
        private const int _WORKSPACE_ID = 1111;

        [SetUp]
        public void SetUp()
        {
            _tabManagerMock = new Mock<ITabManager>();
            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(x => x.CreateProxy<ITabManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_tabManagerMock.Object);

            _sut = new TabService(_servicesMgrFake.Object, Mock.Of<ILogger<TabService>>());
        }

        [Test]
        public void GetTabId_ShouldReturnTabId()
        {
            // Arrange
            const int tabArtifactId = 2222;
            const int objectTypeArtifactId = 3333;
            List<NavigationTabResponse> tabs = new List<NavigationTabResponse>()
            {
                new NavigationTabResponse()
                {
                    ArtifactID = tabArtifactId,
                    ObjectTypeIdentifier = new Securable<ObjectTypeIdentifier>()
                    {
                        Value = new ObjectTypeIdentifier()
                        {
                            ArtifactTypeID = objectTypeArtifactId
                        }
                    }
                }
            };
            _tabManagerMock.Setup(x => x.GetAllNavigationTabs(_WORKSPACE_ID)).ReturnsAsync(tabs);

            // Act
            int actualTabId = _sut.GetTabId(_WORKSPACE_ID, objectTypeArtifactId);

            // Assert
            actualTabId.Should().Be(tabArtifactId);
        }

        [Test]
        public void GetTabId_ShouldThrow_WhenTabForObjectTypeNotExists()
        {
            // Arrange
            const int objectTypeArtifactId = 3333;
            _tabManagerMock.Setup(x => x.GetAllNavigationTabs(_WORKSPACE_ID))
                .ReturnsAsync(new List<NavigationTabResponse>());

            // Act
            System.Action action = () => _sut.GetTabId(_WORKSPACE_ID, objectTypeArtifactId);

            // Assert
            action.ShouldThrow<NotFoundException>();
        }
    }
}
