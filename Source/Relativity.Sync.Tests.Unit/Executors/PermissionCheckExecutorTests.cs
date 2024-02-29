using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.PermissionCheck;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class PermissionCheckExecutorTests
    {
        private Mock<IPermissionCheck> _enabledPermissionCheckMock;
        private Mock<IPermissionCheck> _disabledPermissionCheckMock;
        private PermissionCheckExecutor _sut;

        [SetUp]
        public void Setup()
        {
            _enabledPermissionCheckMock = new Mock<IPermissionCheck>();
            _disabledPermissionCheckMock = new Mock<IPermissionCheck>();

            _disabledPermissionCheckMock.Setup(x => x.ShouldValidate(It.IsAny<ISyncPipeline>())).Returns(false);

            _enabledPermissionCheckMock.Setup(x => x.ShouldValidate(It.IsAny<ISyncPipeline>())).Returns(true);
            _enabledPermissionCheckMock.Setup(x => x.ValidateAsync(It.IsAny<IPermissionsCheckConfiguration>()))
                .ReturnsAsync(new ValidationResult());

            _sut = new PermissionCheckExecutor(new[] { _disabledPermissionCheckMock.Object, _enabledPermissionCheckMock.Object }, new Mock<IPipelineSelector>().Object);
        }

        [Test]
        public async Task ValidateAsync_ShouldRespect_PermissionCheck_ShouldValidate()
        {
            // Act
            await _sut.ExecuteAsync(new ConfigurationStub(), CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _disabledPermissionCheckMock.Verify(x => x.ShouldValidate(It.IsAny<ISyncPipeline>()), Times.Once);
            _enabledPermissionCheckMock.Verify(x => x.ShouldValidate(It.IsAny<ISyncPipeline>()), Times.Once);

            _disabledPermissionCheckMock.Verify(x => x.ValidateAsync(It.IsAny<IPermissionsCheckConfiguration>()), Times.Never);
            _enabledPermissionCheckMock.Verify(x => x.ValidateAsync(It.IsAny<IPermissionsCheckConfiguration>()), Times.Once);
        }
    }
}
