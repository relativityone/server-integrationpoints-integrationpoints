using System;
using FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Logger;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.Relativity.ImportAPI;
using kCura.WinEDDS.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class ImportApiFactoryTests
    {
        private const string _LOCAL_INSTANCE_ADDRESS = "http://instance-address.relativity.com/Relativity";
        private const string _LOCAL_INVALID_INSTANCE_ADDRESS = "http://fake-invalid-address.com/Relativity";

        private ImportApiFactory _sut;

        public ImportApiFactoryTests()
        {
            var retryHandlerFactoryMock = new Mock<IRetryHandlerFactory>();
            var retryHandler = new RetryHandler(new Mock<IAPILog>().Object, 1, 1);
            retryHandlerFactoryMock.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<ushort>())).Returns(retryHandler);

            var importApiBuilderMock = new Mock<IImportApiBuilder>();
            importApiBuilderMock.Setup(x => x.CreateImportAPI(_LOCAL_INSTANCE_ADDRESS, It.IsAny<int>()))
                .Returns(new Mock<IImportAPI>().Object);
            importApiBuilderMock.Setup(x => x.CreateImportAPI(_LOCAL_INVALID_INSTANCE_ADDRESS, It.IsAny<int>()))
                .Throws(new InvalidLoginException());

            _sut = new ImportApiFactory(new Mock<IInstanceSettingsManager>().Object, importApiBuilderMock.Object, retryHandlerFactoryMock.Object, PrepareLoggerStub());
        }

        [Test]
        public void GetImportAPI_GoldFlow()
        {
            // act
            var importApi = _sut.GetImportAPI(_LOCAL_INSTANCE_ADDRESS);

            // assert
            importApi.Should().NotBeNull();
        }

        [Test]
        public void GetImportAPI_ShouldThrowIntegrationPointsException_WhenIAPICannotLogIn()
        {
            // act & assert
            Assert.Throws<IntegrationPointsException>(() => _sut.GetImportAPI(_LOCAL_INVALID_INSTANCE_ADDRESS));
        }

        private static ILogger<ImportApiFactory> PrepareLoggerStub()
        {
            Mock<IAPILog> loggerStub = new Mock<IAPILog>();
            Mock<ISerilogLoggerInstrumentationService> serilogLoggerInstrumentationStub =
                new Mock<ISerilogLoggerInstrumentationService>();
            loggerStub.Setup(m => m.ForContext<ImportApiFactory>()).Returns(loggerStub.Object);
            return new Logger<ImportApiFactory>(loggerStub.Object, serilogLoggerInstrumentationStub.Object);
        }
    }
}
