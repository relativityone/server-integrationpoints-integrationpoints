using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    internal class CustomProviderFlowCheckTests
    {
        private Mock<IToggleProvider> _toggleProviderFake;
        private Mock<ISerializer> _serializerFake;
        private IFixture _fxt;
        private CustomProviderFlowCheck _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _toggleProviderFake = new Mock<IToggleProvider>();

            _serializerFake = new Mock<ISerializer>();
            
            Mock<IAPILog> log = new Mock<IAPILog>();

            _sut = new CustomProviderFlowCheck(
                _toggleProviderFake.Object,
                _serializerFake.Object,
                log.Object);
        }

        [Test]
        public async Task ShouldBeUsedAsync_ShouldReturnTrue_WhenCriteriaAreMet()
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();

            SetupNewCustomProviderToggle(true);
            SetupManagersLinkingConfiguration(false);

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

            SetupManagersLinkingConfiguration(false);

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

            SetupNewCustomProviderToggle(true);

            SetupManagersLinkingConfiguration(true);

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

        private void SetupManagersLinkingConfiguration(bool value)
        {
            ImportSettings settings = _fxt.Create<ImportSettings>();
            settings.EntityManagerFieldContainsLink = value;

            _serializerFake.Setup(x => x.Deserialize<ImportSettings>(It.IsAny<string>()))
                .Returns(settings);
        }
    }
}
