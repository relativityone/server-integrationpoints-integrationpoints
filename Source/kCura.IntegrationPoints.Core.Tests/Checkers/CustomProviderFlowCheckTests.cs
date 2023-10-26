using System;
using AutoFixture;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Checkers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
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
        private Mock<IIntegrationPointService> _integrationPointServiceFake;
        private IFixture _fxt;
        private CustomProviderFlowCheck _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _toggleProviderFake = new Mock<IRipToggleProvider>();
            _integrationPointServiceFake = new Mock<IIntegrationPointService>();

            Mock<IAPILog> log = new Mock<IAPILog>();

            _sut = new CustomProviderFlowCheck(
                _toggleProviderFake.Object,
                _integrationPointServiceFake.Object,
                log.Object);
        }

        [TestCase(true, 10, ProviderType.FTP)]
        [TestCase(false, 1000064, ProviderType.FTP)]
        [TestCase(true, 10, ProviderType.LDAP)]
        [TestCase(false, 1000064, ProviderType.LDAP)]
        [TestCase(true, 10, ProviderType.Other)]
        [TestCase(false, 1000064, ProviderType.Other)]
        public void ShouldBeUsedWithArtifactId_ShouldReturnTrue_WhenCriteriaAreMet(bool entityManagerFieldContainsLink, int artifactTypeId, ProviderType providerType)
        {
            // Arrange
            int integrationPointArtifactId = 1324;
            DestinationConfiguration destinationConfiguration = _fxt.Create<DestinationConfiguration>();
            destinationConfiguration.EntityManagerFieldContainsLink = entityManagerFieldContainsLink;
            destinationConfiguration.ArtifactTypeId = artifactTypeId;
            SetupNewCustomProviderToggle(true);

            _integrationPointServiceFake.Setup(x => x.GetDestinationConfiguration(integrationPointArtifactId))
                .Returns(destinationConfiguration);

            // Act
            bool result = _sut.ShouldBeUsed(integrationPointArtifactId, providerType);

            // Assert
            result.Should().BeTrue();
        }

        [TestCase(true, 10, ProviderType.Relativity)]
        [TestCase(false, 1000064, ProviderType.Relativity)]
        [TestCase(true, 10, ProviderType.ImportLoadFile)]
        [TestCase(false, 1000064, ProviderType.ImportLoadFile)]
        [TestCase(true, 10, ProviderType.LoadFile)]
        [TestCase(false, 1000064, ProviderType.LoadFile)]
        [TestCase(true, 1000064, ProviderType.FTP)]
        public void ShouldNotBeUsedWithArtifactId_ShouldReturnFalse_WhenCriteriaAreNotMet(bool entityManagerFieldContainsLink, int artifactTypeId, ProviderType providerType)
        {
            // Arrange
            int integrationPointArtifactId = 1324;
            DestinationConfiguration destinationConfiguration = _fxt.Create<DestinationConfiguration>();
            destinationConfiguration.EntityManagerFieldContainsLink = entityManagerFieldContainsLink;
            destinationConfiguration.ArtifactTypeId = artifactTypeId;
            SetupNewCustomProviderToggle(true);

            _integrationPointServiceFake.Setup(x => x.GetDestinationConfiguration(integrationPointArtifactId))
                .Returns(destinationConfiguration);

            // Act
            bool result = _sut.ShouldBeUsed(integrationPointArtifactId, providerType);

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(true, 10)]
        [TestCase(false, 1000064)]
        public void ShouldBeUsed_ShouldReturnTrue_WhenCriteriaAreMet(bool entityManagerFieldContainsLink, int artifactTypeId)
        {
            // Arrange
            DestinationConfiguration destinationConfiguration = _fxt.Create<DestinationConfiguration>();
            destinationConfiguration.EntityManagerFieldContainsLink = entityManagerFieldContainsLink;
            destinationConfiguration.ArtifactTypeId = artifactTypeId;
            SetupNewCustomProviderToggle(true);

            // Act
            bool result = _sut.ShouldBeUsed(destinationConfiguration);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_WhenToggleIsDisabled()
        {
            // Arrange
            DestinationConfiguration destinationConfiguration = _fxt.Create<DestinationConfiguration>();
            destinationConfiguration.EntityManagerFieldContainsLink = false;
            SetupNewCustomProviderToggle(false);

            // Act
            bool result = _sut.ShouldBeUsed(destinationConfiguration);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_WhenManagersLinkingAndNonDocumentTransferType()
        {
            // Arrange
            DestinationConfiguration destinationConfiguration = _fxt.Create<DestinationConfiguration>();
            destinationConfiguration.EntityManagerFieldContainsLink = true;
            destinationConfiguration.ArtifactTypeId = 1000064;
            SetupNewCustomProviderToggle(true);

            // Act
            bool result = _sut.ShouldBeUsed(destinationConfiguration);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_WhenExceptionThrown()
        {
            // Arrange
            DestinationConfiguration destinationConfiguration = _fxt.Create<DestinationConfiguration>();

            _toggleProviderFake.Setup(x => x.IsEnabled<EnableImportApiV2ForCustomProvidersToggle>())
                .Throws<Exception>();

            // Act
            bool result = _sut.ShouldBeUsed(destinationConfiguration);

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
