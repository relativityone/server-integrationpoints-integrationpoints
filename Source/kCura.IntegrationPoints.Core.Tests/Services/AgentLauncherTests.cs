using System;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.HostingBridge.V1.AgentStatusManager;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture]
    [Category("Unit")]
    public class AgentLauncherTests : TestBase
    {
        private Mock<IToggleProvider> _toggleProviderMock;
        private Mock<IAgentStatusManagerService> _agentStatusManagerServiceMock;
        private Mock<IServicesMgr> _servicesMgrMock;
        private Mock<IAPILog> _logMock;
        private Guid _agentGuid;
        private AgentLauncher _sut;

        public override void SetUp()
        {
            _toggleProviderMock = new Mock<IToggleProvider>();
            _agentStatusManagerServiceMock = new Mock<IAgentStatusManagerService>();
            _servicesMgrMock = new Mock<IServicesMgr>();
            _logMock = new Mock<IAPILog>();

            _servicesMgrMock.Setup(x => x.CreateProxy<IAgentStatusManagerService>(It.IsAny<ExecutionIdentity>())).Returns(_agentStatusManagerServiceMock.Object);
            _agentGuid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

            _sut = new AgentLauncher(_servicesMgrMock.Object, _toggleProviderMock.Object, _logMock.Object);
        }

        [Test]
        public async Task LaunchAgentAsync_ShouldCallKepler_WhenToggleIsEnabled()
        {
            // Arrange
            _toggleProviderMock.Setup(x => x.IsEnabled<TriggerAgentLaunchOnJobRunToggle>()).Returns(true);

            // Act
            await _sut.LaunchAgentAsync();

            // Assert
            _agentStatusManagerServiceMock.Verify(x => x.StartAgentAsync(_agentGuid), Times.Once);
        }

        [Test]
        public async Task LaunchAgentAsync_ShouldNotCallKepler_WhenToggleIsDisabled()
        {
            // Arrange
            _toggleProviderMock.Setup(x => x.IsEnabled<TriggerAgentLaunchOnJobRunToggle>()).Returns(false);

            // Act
            await _sut.LaunchAgentAsync();

            // Assert
            _agentStatusManagerServiceMock.Verify(x => x.StartAgentAsync(_agentGuid), Times.Never);
        }
    }
}
