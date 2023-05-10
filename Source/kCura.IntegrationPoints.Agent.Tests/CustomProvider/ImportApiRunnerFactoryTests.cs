using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    internal class ImportApiRunnerFactoryTests
    {
        private Mock<IWindsorContainer> _windsorContainerMock;
        private ImportApiRunnerFactory _sut;

        [SetUp]
        public void SetUp()
        {
            _windsorContainerMock = new Mock<IWindsorContainer>();
            _windsorContainerMock.Setup(x => x.Resolve<DocumentImportApiRunner>())
                .Returns(new Mock<DocumentImportApiRunner>().Object);
            _windsorContainerMock.Setup(x => x.Resolve<RdoImportApiRunner>())
                .Returns(new Mock<RdoImportApiRunner>().Object);

            _sut = new ImportApiRunnerFactory(
                _windsorContainerMock.Object,
                new Mock<IAPILog>().Object);
        }

        [TestCase(true, typeof(DocumentImportApiRunner))]
        [TestCase(false, typeof(RdoImportApiRunner))]
        public void BuildRunner_ShouldReturnProperRunnerType(bool isDocumentTransfer, Type expectedRunnerType)
        {
            // Arrange
            var importSettings = new DestinationConfiguration
            {
                ArtifactTypeId = isDocumentTransfer
                    ? (int)ArtifactType.Document
                    : 12345
            };

            // Act
            IImportApiRunner runner = _sut.BuildRunner(importSettings);

            // Assert
            Assert.IsInstanceOf(expectedRunnerType, runner);
        }
    }
}
