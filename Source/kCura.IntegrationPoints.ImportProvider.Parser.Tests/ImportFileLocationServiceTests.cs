using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;
using NSubstitute;
using System;
using SystemInterface.IO;
using kCura.IntegrationPoints.Data;
using Moq;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture, Category("Unit"), Category("ImportProvider")]
    public class ImportFileLocationServiceTests : TestBase
    {
        private const int _IP_ARTIFACT_ID = 1004242;
        private const string _LOAD_FILE_PATH = @"DataTransfer\Import\example-load-file.csv";
        private const string _WORKSPACE_ROOT_LOCATION = @"\\example.host.name\fileshare\EDDS-example-app-id";
        private const string _DATA_TRANSFER_IMPORT_LOCATION = @"DataTransfer\Import";
        private const string _IP_NAME = @"Example-IP-Name";
        private const string _ERROR_FILE_LOCATION =
            @"\\example.host.name\fileshare\EDDS-example-app-id\DataTransfer\Import\Error_Files\Example-IP-Name-1004242-Error_file.csv";
        private const string _LOAD_FILE_LOCATION =
            @"\\example.host.name\fileshare\EDDS-example-app-id\DataTransfer\Import\example-load-file.csv";
        private const int _LOAD_FILE_SIZE = 1000;
        private readonly DateTime _LOAD_FILE_LAST_MODIFIED_DATE = new DateTime(2020, 1, 1);
        private IDataTransferLocationService _locationService;
        private ISerializer _serializer;
        private IDirectory _directoryHelper;
        private IFileInfoFactory _fileInfoFactory;
        private ImportProviderSettings _providerSettings;
        private IntegrationPointDto _integrationPoint;
        private Mock<IFileInfo> _loadFileInfo;

        [SetUp]
        public override void SetUp()
        {
            _integrationPoint = new IntegrationPointDto();
            _integrationPoint.Name = _IP_NAME;
            _integrationPoint.ArtifactId = _IP_ARTIFACT_ID;
            _integrationPoint.SourceConfiguration = string.Empty;
            _integrationPoint.DestinationConfiguration = new DestinationConfiguration { CaseArtifactId = -1};

            _providerSettings = new ImportProviderSettings();

            _providerSettings.LoadFile = _LOAD_FILE_PATH;

            _locationService = Substitute.For<IDataTransferLocationService>();
            _serializer = Substitute.For<ISerializer>();
            _directoryHelper = Substitute.For<IDirectory>();
            _fileInfoFactory = Substitute.For<IFileInfoFactory>();

            _loadFileInfo = new Mock<IFileInfo>();
            _loadFileInfo.SetupGet(x => x.Length).Returns(_LOAD_FILE_SIZE);
            _loadFileInfo.SetupGet(x => x.LastWriteTimeUtc)
                .Returns(new DateTimeWrap(_LOAD_FILE_LAST_MODIFIED_DATE));

            _fileInfoFactory.Create(_LOAD_FILE_LOCATION).Returns(_loadFileInfo.Object);

            _serializer.Deserialize<ImportProviderSettings>(Arg.Any<string>()).ReturnsForAnyArgs(_providerSettings);
            _serializer.Deserialize<DestinationConfiguration>(Arg.Any<string>()).ReturnsForAnyArgs(_integrationPoint.DestinationConfiguration);
            _locationService.GetWorkspaceFileLocationRootPath(Arg.Any<int>()).ReturnsForAnyArgs(_WORKSPACE_ROOT_LOCATION);
            _locationService.GetDefaultRelativeLocationFor(Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid).Returns(_DATA_TRANSFER_IMPORT_LOCATION);
        }

        [Test]
        public void ItShouldReturnTheCorrectErrorFilePath()
        {
            // Arrange
            _directoryHelper.Exists(Arg.Any<string>()).ReturnsForAnyArgs(true);
            ImportFileLocationService locationService = new ImportFileLocationService(
                _locationService,
                _serializer,
                _directoryHelper,
                _fileInfoFactory);

            // Act
            string generatedErrorFilePath = locationService.ErrorFilePath(_integrationPoint.ArtifactId, _integrationPoint.Name, _integrationPoint.SourceConfiguration, _integrationPoint.DestinationConfiguration);

            // Assert
            Assert.AreEqual(_ERROR_FILE_LOCATION, generatedErrorFilePath);
        }

        [Test]
        public void ItShouldReturnTheCorrectLoadFileInfo()
        {
            // Arrange
            ImportFileLocationService locationService = new ImportFileLocationService(
                _locationService,
                _serializer,
                _directoryHelper,
                _fileInfoFactory);

            // Act
            LoadFileInfo loadFile = locationService.LoadFileInfo(_integrationPoint.SourceConfiguration, _integrationPoint.DestinationConfiguration);

            // Assert
            Assert.AreEqual(_LOAD_FILE_LOCATION, loadFile.FullPath);
            Assert.AreEqual(_LOAD_FILE_SIZE, loadFile.Size);
            Assert.AreEqual(_LOAD_FILE_LAST_MODIFIED_DATE, loadFile.LastModifiedDate);
        }

        [Test]
        public void ItShouldThrowWhenLoadFileSettingIsARootedPath()
        {
            _providerSettings.LoadFile = @"\\badshare\badpath\badfile.csv";
            // Arrange
            ImportFileLocationService locationService = new ImportFileLocationService(
                _locationService,
                _serializer,
                _directoryHelper,
                _fileInfoFactory);

            // Assert that it throws because we should not have a rooted load file path in the settings object
            // This would be a security vulnerability
            Assert.Throws<Exception>(() => locationService.LoadFileInfo(_integrationPoint.SourceConfiguration, _integrationPoint.DestinationConfiguration));
        }

        [Test]
        public void ItShouldThrowWhenNotInTheDataTransferLocation()
        {
            _providerSettings.LoadFile = @"badshare\..\..\..\..\badpath\badfile.csv";
            // Arrange
            ImportFileLocationService locationService = new ImportFileLocationService(
                _locationService,
                _serializer,
                _directoryHelper,
                _fileInfoFactory);

            // Assert that it throws because we should not have a load file path that doesn't point to the DataTransfer\Import path
            Assert.Throws<Exception>(() => locationService.LoadFileInfo(_integrationPoint.SourceConfiguration, _integrationPoint.DestinationConfiguration));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ItShouldCreateDirectoryIfNecessary(bool directoryExists)
        {
            // Arrange
            _directoryHelper.Exists(Arg.Any<string>()).ReturnsForAnyArgs(directoryExists);
            ImportFileLocationService locationService = new ImportFileLocationService(
                _locationService,
                _serializer,
                _directoryHelper,
                _fileInfoFactory);

            // Act
            locationService.ErrorFilePath(_integrationPoint.ArtifactId, _integrationPoint.Name, _integrationPoint.SourceConfiguration, _integrationPoint.DestinationConfiguration);

            // Assert
            _directoryHelper.Received(directoryExists ? 0 : 1).CreateDirectory(Arg.Any<string>());
        }
    }
}
