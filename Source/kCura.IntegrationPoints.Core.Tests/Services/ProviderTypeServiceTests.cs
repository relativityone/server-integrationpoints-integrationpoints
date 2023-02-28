using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;

using SourceProviders = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders;
using DestinationProviders = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders;
using FluentAssertions;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture, Category("Unit")]
    public class ProviderTypeServiceTests
    {
        [TestCase(SourceProviders.FTP, null, ProviderType.FTP)]
        [TestCase(SourceProviders.LDAP, null, ProviderType.LDAP)]
        [TestCase(SourceProviders.IMPORTLOADFILE, null, ProviderType.ImportLoadFile)]
        [TestCase(SourceProviders.RELATIVITY, DestinationProviders.RELATIVITY, ProviderType.Relativity)]
        [TestCase(SourceProviders.RELATIVITY, DestinationProviders.LOADFILE, ProviderType.LoadFile)]
        public void GetProviderName_ShouldReturnProviderName_BasedOnProviders(string sourceProviderGuid, string destinationProviderGuid,
            ProviderType expectedProvider)
        {
            // Arrange
            string expectedProviderName = expectedProvider.ToString();

            IRelativityObjectManager objectManager = PrepareObjectManager(sourceProviderGuid, destinationProviderGuid);

            IProviderTypeService sut = new ProviderTypeService(objectManager);

            // Act
            string name = sut.GetProviderName(It.IsAny<int>(), It.IsAny<int>());

            // Assert
            name.Should().Be(expectedProviderName);
        }

        [Test]
        public void GetProviderName_ShouldReturnProviderName_WhenCustomProviderIsSelected()
        {
            // Arrange
            string customProviderName = "CustomProvider";
            string customProviderGuid = Guid.NewGuid().ToString();

            IRelativityObjectManager objectManager = PrepareObjectManager(customProviderGuid, It.IsAny<string>(), customProviderName);

            IProviderTypeService sut = new ProviderTypeService(objectManager);

            // Act
            string name = sut.GetProviderName(It.IsAny<int>(), It.IsAny<int>());

            // Assert
            name.Should().Be(customProviderName);
        }

        [Test]
        public void GetProviderName_ShouldReturnTrimmedProviderName_WhenProviderContainsWhiteSpaces()
        {
            // Arrange
            string customProviderName = "Custom Provider";
            string customProviderGuid = Guid.NewGuid().ToString();

            string expectedProviderName = "CustomProvider";

            IRelativityObjectManager objectManager = PrepareObjectManager(customProviderGuid, It.IsAny<string>(), customProviderName);

            IProviderTypeService sut = new ProviderTypeService(objectManager);

            // Act
            string name = sut.GetProviderName(It.IsAny<int>(), It.IsAny<int>());

            // Assert
            name.Should().Be(expectedProviderName);
        }

        [Test]
        public void GetProviderName_ShouldReturnOtherProviderName_WhenProviderNameIsNull()
        {
            // Arrange
            string customProviderName = "";
            string customProviderGuid = Guid.NewGuid().ToString();

            string expectedProviderName = ProviderType.Other.ToString();

            IRelativityObjectManager objectManager = PrepareObjectManager(customProviderGuid, It.IsAny<string>(), customProviderName);

            IProviderTypeService sut = new ProviderTypeService(objectManager);

            // Act
            string name = sut.GetProviderName(It.IsAny<int>(), It.IsAny<int>());

            // Assert
            name.Should().Be(expectedProviderName);
        }

        [TestCase(SourceProviders.FTP, null, ProviderType.FTP)]
        [TestCase(SourceProviders.LDAP, null, ProviderType.LDAP)]
        [TestCase(SourceProviders.IMPORTLOADFILE, null, ProviderType.ImportLoadFile)]
        [TestCase(SourceProviders.RELATIVITY, DestinationProviders.RELATIVITY, ProviderType.Relativity)]
        [TestCase(SourceProviders.RELATIVITY, DestinationProviders.LOADFILE, ProviderType.LoadFile)]
        [TestCase("1C525F80-6EF2-48E9-9135-42DC678FBAEE", null, ProviderType.Other)]
        public void GetProviderType_ShouldReturnProviderType_BasedOnProviders(string sourceProviderGuid, string destinationProviderGuid,
            ProviderType expectedProvider)
        {
            // Arrange
            IRelativityObjectManager objectManager = PrepareObjectManager(sourceProviderGuid, destinationProviderGuid);

            IProviderTypeService sut = new ProviderTypeService(objectManager);

            // Act
            ProviderType type = sut.GetProviderType(It.IsAny<int>(), It.IsAny<int>());

            // Assert
            type.Should().Be(expectedProvider);
        }

        private IRelativityObjectManager PrepareObjectManager(string sourceProviderGuid, string destinationProviderGuid,
            string sourceProviderName = "", string destinationProviderName = "")
        {
            Mock<IRelativityObjectManager> objectManagerFake = new Mock<IRelativityObjectManager>();
            objectManagerFake.Setup(x => x.Read<SourceProvider>(It.IsAny<int>(), It.IsAny<ExecutionIdentity>()))
                .Returns(new SourceProvider() { Identifier = sourceProviderGuid, Name = sourceProviderName });

            objectManagerFake.Setup(x => x.Read<DestinationProvider>(It.IsAny<int>(), It.IsAny<ExecutionIdentity>()))
                .Returns(new DestinationProvider() { Identifier = destinationProviderGuid, Name = destinationProviderName });

            return objectManagerFake.Object;
        }
    }
}
