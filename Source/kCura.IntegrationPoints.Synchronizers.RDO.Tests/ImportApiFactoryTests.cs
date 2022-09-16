using System;
using kCura.IntegrationPoints.Data.Logging;
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
    public class ImportApiFactoryTests : ImportApiFactory
    {
        private bool _shouldThrowInvalidLoginException;
        private bool _shouldThrowInvalidOperationException;

        private const string _LOCAL_INSTANCE_ADDRESS = "http://instance-address.relativity.com/Relativity";
        private const int _FEDERATED_INSTANCE_ARTIFACTID = 666;

        public ImportApiFactoryTests()
            : base(null, new Mock<ISystemEventLoggingService>().Object, new Mock<IInstanceSettingsManager>().Object, PrepareLoggerStub())
        { }

        [Test]
        public void GetImportAPI_ShouldThrowNotSupportedException_WhenInstanceToInstance()
        {
            // arrange
            ImportSettings settings = new ImportSettings
            {
                WebServiceURL = _LOCAL_INSTANCE_ADDRESS,
                FederatedInstanceArtifactId = _FEDERATED_INSTANCE_ARTIFACTID
            };

            // act 
            TestDelegate createImportApiAction = () => GetImportAPI(settings);

            // assert
            Assert.Throws<NotSupportedException>(createImportApiAction);
        }
        
        [Test]
        public void GetImportAPI_ShouldThrowIntegrationPointsException_WhenIAPICannotLogIn()
        {
            // arrange
            ImportSettings settings = new ImportSettings();
            _shouldThrowInvalidLoginException = true;
            _shouldThrowInvalidOperationException = false;

            // act & assert
            Assert.Throws<IntegrationPointsException>(() => GetImportAPI(settings));
        }

        [Test]
        public void GetImportAPI_ShouldRethrowInvalidOperationException()
        {
            // arrange
            ImportSettings settings = new ImportSettings();
            _shouldThrowInvalidLoginException = false;
            _shouldThrowInvalidOperationException = true;

            // act & assert
            Assert.Throws<InvalidOperationException>(() => GetImportAPI(settings));
        }

        protected override IImportAPI CreateImportAPI(string webServiceUrl)
        {
            if (_shouldThrowInvalidLoginException)
            {
                throw new InvalidLoginException("Login failed.");
            }
            else if (_shouldThrowInvalidOperationException)
            {
                throw new InvalidOperationException();
            }
            else
            {
                return new Mock<IImportAPI>().Object;
            }
        }

        private static IAPILog PrepareLoggerStub()
        {
            Mock<IAPILog> loggerStub = new Mock<IAPILog>();
            loggerStub.Setup(m => m.ForContext<ImportApiFactory>()).Returns(loggerStub.Object);

            return loggerStub.Object;
        }
    }
}
