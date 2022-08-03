using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
    [TestFixture, Category("Unit")]
    public class ExportServiceFactoryTests : TestBase
    {
        private ExportServiceFactory _sut;
        private Mock<ExportServiceFactory.CreateWebApiServiceFactoryDelegate> _createWebApiServiceFactoryDelegateMock;
        private Mock<ExportServiceFactory.CreateCoreServiceFactoryDelegate> _createCoreServiceFactoryDelegate;
        private Mock<IInstanceSettingRepository> _instanceSettingRepository;
        private ExportDataContext _exportDataContext;

        private const int _exportTypeArtifactID = 1234;

        [SetUp]
        public override void SetUp()
        {
            _instanceSettingRepository = new Mock<IInstanceSettingRepository>();

            Mock<IAPILog> logger = new Mock<IAPILog>
            {
                DefaultValue = DefaultValue.Mock
            };
            _exportDataContext = new ExportDataContext
            {
                ExportFile = new ExtendedExportFile(_exportTypeArtifactID)
            };

            _createWebApiServiceFactoryDelegateMock = new Mock<ExportServiceFactory.CreateWebApiServiceFactoryDelegate>();
            _createCoreServiceFactoryDelegate = new Mock<ExportServiceFactory.CreateCoreServiceFactoryDelegate>();

            _sut = new ExportServiceFactory(
                logger.Object,
                _instanceSettingRepository.Object,
                _createWebApiServiceFactoryDelegateMock.Object,
                _createCoreServiceFactoryDelegate.Object
            );
        }

        [Test]
        [TestCase("True", true)]
        [TestCase("False", false)]
        [TestCase("invalid boolean string", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void ShouldCreateServiceFactoryBasedOnInstanceSettingValue(string useCoreApiConfig, bool isCoreServiceFactoryExpected)
        {
            // arrange
            _instanceSettingRepository
                .Setup(x => x.GetConfigurationValue(
                    Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
                    Constants.REPLACE_WEB_API_WITH_EXPORT_CORE))
                .Returns(useCoreApiConfig);

            // act
            _sut.Create(_exportDataContext);

            // assert
            _createWebApiServiceFactoryDelegateMock.Verify(x => x(It.IsAny<ExportFile>()), Times.Once);
            Times expectedNumberOfCallsToCreateCoreServiceFactoryDelegate = isCoreServiceFactoryExpected
                ? Times.Once()
                : Times.Never();
            _createCoreServiceFactoryDelegate.Verify(
                x => x(It.IsAny<ExportFile>(), It.IsAny<IServiceFactory>()),
                expectedNumberOfCallsToCreateCoreServiceFactoryDelegate);
        }
    }
}