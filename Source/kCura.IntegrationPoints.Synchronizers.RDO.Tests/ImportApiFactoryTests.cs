using FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Logger;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.Relativity.ImportAPI;
using kCura.WinEDDS.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Serilog;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class ImportApiFactoryTests
    {
        private const string _LOCAL_INSTANCE_ADDRESS = "http://instance-address.relativity.com/Relativity";
        private const string _LOCAL_INVALID_INSTANCE_ADDRESS = "http://fake-invalid-address.com/Relativity";

        private readonly Mock<IWebApiConfig> _webApiConfigMock;

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

            _webApiConfigMock = new Mock<IWebApiConfig>();
            _webApiConfigMock.Setup(x => x.WebApiUrl).Returns(_LOCAL_INSTANCE_ADDRESS);

            _sut = new ImportApiFactory(_webApiConfigMock.Object, new Mock<IInstanceSettingsManager>().Object, importApiBuilderMock.Object, retryHandlerFactoryMock.Object, new Mock<ILogger<ImportApiFactory>>().Object);
        }

        [Test]
        public void GetImportAPI_GoldFlow()
        {
            // act
            var importApi = _sut.GetImportAPI();

            // assert
            importApi.Should().NotBeNull();
        }

        [Test]
        public void GetImportAPI_ShouldThrowIntegrationPointsException_WhenIAPICannotLogIn()
        {
            _webApiConfigMock.Setup(x => x.WebApiUrl).Returns(_LOCAL_INVALID_INSTANCE_ADDRESS);

            // act & assert
            Assert.Throws<IntegrationPointsException>(() => _sut.GetImportAPI());
        }
    }
}
