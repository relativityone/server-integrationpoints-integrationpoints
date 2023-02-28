using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    internal class CustomProviderFlowCheckTests
    {
        private Mock<IToggleProvider> _toggleProviderFake;
        private Mock<ISerializer> _serializerFake;
        private Mock<IRelativityObjectManager> _objectManagerFake;
        private IFixture _fxt;
        private CustomProviderFlowCheck _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _toggleProviderFake = new Mock<IToggleProvider>();

            _serializerFake = new Mock<ISerializer>();

            _objectManagerFake = new Mock<IRelativityObjectManager>();

            Mock<IAPILog> log = new Mock<IAPILog>();

            _sut = new CustomProviderFlowCheck(
                _toggleProviderFake.Object,
                _objectManagerFake.Object,
                _serializerFake.Object,
                log.Object);
        }

        [Test]
        public async Task ShouldBeUsedAsync_ShouldReturnTrue_WhenCriteriaAreMet()
        {
            // Arrange
            Guid notEntityGuid = new Guid("D338C2A3-1C4F-4031-B2F0-B58FC7FAA964");

            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();

            SetupNewCustomProviderToggle(true);

            SetupObjectTypeReadFromConfiguration(notEntityGuid);

            // Act
            bool result = await _sut.ShouldBeUsedAsync(integrationPoint).ConfigureAwait(false);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ShouldBeUsedAsync_ShouldReturnFalse_WhenToggleIsDisabled()
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();

            SetupNewCustomProviderToggle(false);

            SetupObjectTypeReadFromConfiguration(It.IsAny<Guid>());

            // Act
            bool result = await _sut.ShouldBeUsedAsync(integrationPoint).ConfigureAwait(false);

            // Assert
            result.Should().BeFalse();

            _objectManagerFake.Verify(
                x => x.QueryAsync(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<ExecutionIdentity>()),
                Times.Never);
        }

        [Test]
        public async Task ShouldBeUsedAsync_ShouldReturnFalse_WhenEntityImport()
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();

            SetupNewCustomProviderToggle(true);

            SetupObjectTypeReadFromConfiguration(ObjectTypeGuids.Entity);

            // Act
            bool result = await _sut.ShouldBeUsedAsync(integrationPoint).ConfigureAwait(false);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task ShouldBeUsedAsync_ShouldReturnFalse_WhenExceptionThrown()
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();

            _toggleProviderFake.Setup(x => x.IsEnabled<EnableImportApiV2ForCustomProvidersToggle>())
                .Throws<Exception>();

            // Act
            bool result = await _sut.ShouldBeUsedAsync(integrationPoint).ConfigureAwait(false);

            // Assert
            result.Should().BeFalse();
        }

        private void SetupNewCustomProviderToggle(bool value)
        {
            _toggleProviderFake
                .Setup(x => x.IsEnabledAsync<EnableImportApiV2ForCustomProvidersToggle>())
                .ReturnsAsync(value);
        }

        private void SetupObjectTypeReadFromConfiguration(Guid objectTypeGuid)
        {
            ImportSettings settings = _fxt.Create<ImportSettings>();

            _serializerFake.Setup(x => x.Deserialize<ImportSettings>(It.IsAny<string>()))
                .Returns(settings);

            _objectManagerFake.Setup(x => x.QueryAsync(
                    It.Is<QueryRequest>(q => q.Condition.Contains(settings.ArtifactTypeId.ToString())),
                    ExecutionIdentity.System))
                .ReturnsAsync(new List<RelativityObject>()
                {
                    new RelativityObject { Guids = new List<Guid>() { objectTypeGuid } }
                });
        }
    }
}
