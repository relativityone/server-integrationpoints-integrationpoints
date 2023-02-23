using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using Moq;
using NUnit.Framework;
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

        [TestCase(ImportApiFlowEnum.Document, typeof(DocumentImportApiRunner))]
        [TestCase(ImportApiFlowEnum.Rdo, typeof(RdoImportApiRunner))]
        public void BuildRunner_ShouldReturnProperRunnerType(ImportApiFlowEnum flow, Type expectedRunnerType)
        {
            // Act
            IImportApiRunner runner = _sut.BuildRunner(flow);

            // Assert
            Assert.IsInstanceOf(expectedRunnerType, runner);
        }
    }
}
