using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
    [TestFixture, Category("Unit")]
    public class ExportServiceFactoryTests : TestBase
    {
        private ExportServiceFactory _sut;
        private Mock<ExportServiceFactory.CreateWebApiServiceFactoryDelegate> _createWebApiServiceFactoryDelegateMock;
        private Mock<ExportServiceFactory.CreateCoreServiceFactoryDelegate> _createCoreServiceFactoryDelegate;
        private ExportDataContext _exportDataContext;
        private const int _exportTypeArtifactID = 1234;

        [SetUp]
        public override void SetUp()
        {
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
                _createWebApiServiceFactoryDelegateMock.Object,
                _createCoreServiceFactoryDelegate.Object
            );
        }

        [Test]
        public void ShouldCreateServiceFactory()
        {
            // act
            _sut.Create(_exportDataContext);

            // assert
            _createWebApiServiceFactoryDelegateMock.Verify(x => x(It.IsAny<ExportFile>()), Times.Once);
            Times expectedNumberOfCallsToCreateCoreServiceFactoryDelegate = Times.Once();
            _createCoreServiceFactoryDelegate.Verify(
                x => x(It.IsAny<ExportFile>(), It.IsAny<IServiceFactory>()),
                expectedNumberOfCallsToCreateCoreServiceFactoryDelegate);
        }
    }
}
