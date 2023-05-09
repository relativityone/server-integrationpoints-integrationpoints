using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    internal class CustomProviderFlowCheckTests
    {
        private Mock<IRipToggleProvider> _toggleProviderFake;
        private IFixture _fxt;
        private CustomProviderFlowCheck _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _toggleProviderFake = new Mock<IRipToggleProvider>();

            Mock<IAPILog> log = new Mock<IAPILog>();

            _sut = new CustomProviderFlowCheck(
                _toggleProviderFake.Object,
                log.Object);
        }

        [Test]
        public async Task ShouldBeUsedAsync_ShouldReturnTrue_WhenCriteriaAreMet()
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();
            integrationPoint.DestinationConfiguration.EntityManagerFieldContainsLink = false;
            SetupNewCustomProviderToggle(true);

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
            integrationPoint.DestinationConfiguration.EntityManagerFieldContainsLink = false;
            SetupNewCustomProviderToggle(false);

            // Act
            bool result = await _sut.ShouldBeUsedAsync(integrationPoint).ConfigureAwait(false);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task ShouldBeUsedAsync_ShouldReturnFalse_WhenManagersLinking()
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();
            integrationPoint.DestinationConfiguration.EntityManagerFieldContainsLink = true;
            SetupNewCustomProviderToggle(true);

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

            _toggleProviderFake.Setup(x => x.IsEnabledAsync<EnableImportApiV2ForCustomProvidersToggle>())
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
    }
}
