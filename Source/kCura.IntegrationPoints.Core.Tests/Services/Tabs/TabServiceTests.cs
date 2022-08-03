using System;
using System.Collections.Generic;
using FluentAssertions;
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
        private Mock<IHelper> _helperFake;
        private TabService _sut;

        private const int _WORKSPACE_ID = 1111;

        [SetUp]
        public void SetUp()
        {
            _tabManagerMock = new Mock<ITabManager>();
            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(x => x.CreateProxy<ITabManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_tabManagerMock.Object);
            
            _helperFake = new Mock<IHelper>();
            Mock<IAPILog> logger = new Mock<IAPILog>();
            logger.Setup(x => x.ForContext<TabService>()).Returns(logger.Object);
            Mock<ILogFactory> logFactory = new Mock<ILogFactory>();
            logFactory.Setup(x => x.GetLogger()).Returns(logger.Object);
            _helperFake.Setup(x => x.GetLoggerFactory()).Returns(logFactory.Object);

            _sut = new TabService(_servicesMgrFake.Object, _helperFake.Object);
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