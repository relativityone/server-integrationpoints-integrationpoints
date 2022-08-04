using System;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API.Foundation;
using Relativity.API.Foundation.Repositories;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class AuditRepositoryTests
    {
        private Mock<IExportAuditRepository> _exportAuditRepository;
        private Mock<IExternalServiceInstrumentationProvider> _instrumentationProvider;
        private Mock<IExternalServiceSimpleInstrumentation> _instrumentation;

        private const int _USER_ID = 9;

        [SetUp]
        public void SetUp()
        {
            _exportAuditRepository = new Mock<IExportAuditRepository>();
            _instrumentation = new Mock<IExternalServiceSimpleInstrumentation>();
            _instrumentation.Setup(x => x.Execute(It.IsAny<Func<bool>>())).Returns<Func<bool>>(y => y.Invoke());
            _instrumentationProvider = new Mock<IExternalServiceInstrumentationProvider>();
            _instrumentationProvider.Setup(x => x.CreateSimple(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_instrumentation.Object);
        }

        [Test]
        public void ShouldReturnProperResultWhenCallAuditExportAndInstrumentSuccessfully([Values(true, false)] bool expectedResult)
        {
            //arrange
            _exportAuditRepository.Setup(x => x.CreateAuditForExport(It.IsAny<ExportStatistics>(),_USER_ID)).Returns(expectedResult);
            var auditRepository = new AuditRepository(_exportAuditRepository.Object, _instrumentationProvider.Object);
            var exportStats = new ExportStatistics();

            //act
            bool result = auditRepository.AuditExport(exportStats, _USER_ID);

            //assert
            _exportAuditRepository.Verify(x => x.CreateAuditForExport(exportStats, _USER_ID), Times.Once);
            _instrumentationProvider.Verify(
                x => x.CreateSimple(
                    "API.Foundation",
                    nameof(IExportAuditRepository),
                    nameof(IExportAuditRepository.CreateAuditForExport)), 
                Times.Once);
            _instrumentation.Verify(x => x.Execute(It.IsAny<Func<bool>>()), Times.Once);
            expectedResult.Should().Be(result);
        }

        [Test]
        public void ShouldInstrumentSuccessfullyWhenIExportAuditRepositoryFails()
        {
            //arrange
            _exportAuditRepository.Setup(x => x.CreateAuditForExport(It.IsAny<ExportStatistics>(), _USER_ID)).Throws<Exception>();
            var auditRepository = new AuditRepository(_exportAuditRepository.Object, _instrumentationProvider.Object);
            var exportStats = new ExportStatistics();

            //act
            Action action = () => auditRepository.AuditExport(exportStats, _USER_ID);

            //assert
            action.ShouldThrow<Exception>();
            _exportAuditRepository.Verify(x => x.CreateAuditForExport(exportStats, _USER_ID), Times.Once);
            _instrumentationProvider.Verify(
                x => x.CreateSimple(
                    "API.Foundation",
                    nameof(IExportAuditRepository),
                    nameof(IExportAuditRepository.CreateAuditForExport)),
                Times.Once);
            _instrumentation.Verify(x => x.Execute(It.IsAny<Func<bool>>()), Times.Once);
        }

        [Test]
        public void ShouldReturnFalseWhenCallAuditExportWithNullAndInstrumentSuccessfully()
        {
            //arrange
            _exportAuditRepository.Setup(x => x.CreateAuditForExport(It.IsAny<ExportStatistics>(), _USER_ID)).Returns(false);
            var auditRepository = new AuditRepository(_exportAuditRepository.Object, _instrumentationProvider.Object);

            //act
            bool result = auditRepository.AuditExport(null, _USER_ID);

            //assert
            _exportAuditRepository.Verify(x => x.CreateAuditForExport(null, _USER_ID), Times.Once);
            _instrumentationProvider.Verify(
                x => x.CreateSimple(
                    "API.Foundation",
                    nameof(IExportAuditRepository),
                    nameof(IExportAuditRepository.CreateAuditForExport)),
                Times.Once);
            _instrumentation.Verify(x => x.Execute(It.IsAny<Func<bool>>()), Times.Once);
            result.Should().BeFalse();
        }
    }
}
