using System;
using AutoFixture;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Checkers;
using kCura.IntegrationPoints.Core.Models;
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

        [TestCase(true, 10)]
        [TestCase(false, 1000064)]
        public void ShouldBeUsed_ShouldReturnTrue_WhenCriteriaAreMet(bool entityManagerFieldContainsLink, int artifactTypeId)
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();
            integrationPoint.DestinationConfiguration.EntityManagerFieldContainsLink = entityManagerFieldContainsLink;
            integrationPoint.DestinationConfiguration.ArtifactTypeId = artifactTypeId;
            SetupNewCustomProviderToggle(true);

            // Act
            bool result = _sut.ShouldBeUsed(integrationPoint);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_WhenToggleIsDisabled()
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();
            integrationPoint.DestinationConfiguration.EntityManagerFieldContainsLink = false;
            SetupNewCustomProviderToggle(false);

            // Act
            bool result = _sut.ShouldBeUsed(integrationPoint);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_WhenManagersLinkingAndNonDocumentTransferType()
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();
            integrationPoint.DestinationConfiguration.EntityManagerFieldContainsLink = true;
            integrationPoint.DestinationConfiguration.ArtifactTypeId = 1000064;
            SetupNewCustomProviderToggle(true);

            // Act
            bool result = _sut.ShouldBeUsed(integrationPoint);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_WhenExceptionThrown()
        {
            // Arrange
            IntegrationPointDto integrationPoint = _fxt.Create<IntegrationPointDto>();

            _toggleProviderFake.Setup(x => x.IsEnabled<EnableImportApiV2ForCustomProvidersToggle>())
                .Throws<Exception>();

            // Act
            bool result = _sut.ShouldBeUsed(integrationPoint);

            // Assert
            result.Should().BeFalse();
        }

        private void SetupNewCustomProviderToggle(bool value)
        {
            _toggleProviderFake
                .Setup(x => x.IsEnabled<EnableImportApiV2ForCustomProvidersToggle>())
                .Returns(value);
        }
    }
}
