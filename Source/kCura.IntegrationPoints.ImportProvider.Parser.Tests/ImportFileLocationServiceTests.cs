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

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	[TestFixture, Category("Unit"), Category("ImportProvider")]
	public class ImportFileLocationServiceTests : TestBase
	{
		private const string _LOAD_FILE_PATH = @"DataTransfer\Import\example-load-file.csv";
		private const string _WORKSPACE_ROOT_LOCATION = @"\\example.host.name\fileshare\EDDS-example-app-id";
		private const string _DATA_TRANSFER_IMPORT_LOCATION = @"DataTransfer\Import";
		private const string _IP_NAME = @"Example-IP-Name";
		private const string _ERROR_FILE_LOCATION =
			@"\\example.host.name\fileshare\EDDS-example-app-id\DataTransfer\Import\Error_Files\Example-IP-Name-1004242-Error_file.csv";
		private const string _LOAD_FILE_LOCATION =
			@"\\example.host.name\fileshare\EDDS-example-app-id\DataTransfer\Import\example-load-file.csv";

		private IDataTransferLocationService _locationService;
		private ISerializer _serializer;
		private IDirectory _directoryHelper;
		private ImportProviderSettings _providerSettings;

		private Data.IntegrationPoint _integrationPoint;

		[SetUp]
		public override void SetUp()
		{
			_integrationPoint = new Data.IntegrationPoint();
			_integrationPoint.Name = _IP_NAME;
			_integrationPoint.SourceConfiguration = string.Empty;
			_integrationPoint.DestinationConfiguration = string.Empty;

			_providerSettings = new ImportProviderSettings();
			ImportSettings importApiSettings = new ImportSettings();

			importApiSettings.CaseArtifactId = -1;
			_providerSettings.LoadFile = _LOAD_FILE_PATH;

			_locationService = Substitute.For<IDataTransferLocationService>();
			_serializer = Substitute.For<ISerializer>();
			_directoryHelper = Substitute.For<IDirectory>();

			_serializer.Deserialize<ImportProviderSettings>(Arg.Any<string>()).ReturnsForAnyArgs(_providerSettings);
			_serializer.Deserialize<ImportSettings>(Arg.Any<string>()).ReturnsForAnyArgs(importApiSettings);
			_locationService.GetWorkspaceFileLocationRootPath(Arg.Any<int>()).ReturnsForAnyArgs(_WORKSPACE_ROOT_LOCATION);
			_locationService.GetDefaultRelativeLocationFor(Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid).Returns(_DATA_TRANSFER_IMPORT_LOCATION);
		}

		[Test]
		public void ItShouldReturnTheCorrectErrorFilePath()
		{
			//Arrange
			_directoryHelper.Exists(Arg.Any<string>()).ReturnsForAnyArgs(true);
			ImportFileLocationService locationService = new ImportFileLocationService(
				_locationService,
				_serializer,
				_directoryHelper);

			//Act
			string generatedErrorFilePath = locationService.ErrorFilePath(_integrationPoint);

			//Assert
			Assert.AreEqual(_ERROR_FILE_LOCATION, generatedErrorFilePath);
		}

		[Test]
		public void ItShouldReturnTheCorrectLoadFileFullPath()
		{
			//Arrange
			ImportFileLocationService locationService = new ImportFileLocationService(
				_locationService,
				_serializer,
				_directoryHelper);

			//Act
			LoadFileInfo loadFile = locationService.LoadFileInfo(_integrationPoint);

			//Assert
			Assert.AreEqual(_LOAD_FILE_LOCATION, loadFile.FullPath);
		}

		[Test]
		public void ItShouldThrowWhenLoadFileSettingIsARootedPath()
		{
			_providerSettings.LoadFile = @"\\badshare\badpath\badfile.csv";
			//Arrange
			ImportFileLocationService locationService = new ImportFileLocationService(
				_locationService,
				_serializer,
				_directoryHelper);

			//Assert that it throws because we should not have a rooted load file path in the settings object
			//This would be a security vulnerability
			Assert.Throws<Exception>(() => locationService.LoadFileInfo(_integrationPoint));
		}

		[Test]
		public void ItShouldThrowWhenNotInTheDataTransferLocation()
		{
			_providerSettings.LoadFile = @"badshare\..\..\..\..\badpath\badfile.csv";
			//Arrange
			ImportFileLocationService locationService = new ImportFileLocationService(
				_locationService,
				_serializer,
				_directoryHelper);

			//Assert that it throws because we should not have a load file path that doesn't point to the DataTransfer\Import path
			Assert.Throws<Exception>(() => locationService.LoadFileInfo(_integrationPoint));
		}

		[TestCase(false)]
		[TestCase(true)]
		public void ItShouldCreateDirectoryIfNecessary(bool directoryExists)
		{
			//Arrange
			_directoryHelper.Exists(Arg.Any<string>()).ReturnsForAnyArgs(directoryExists);
			ImportFileLocationService locationService = new ImportFileLocationService(
				_locationService,
				_serializer,
				_directoryHelper);

			//Act
			locationService.ErrorFilePath(_integrationPoint);

			//Assert
			_directoryHelper.Received(directoryExists ? 0 : 1).CreateDirectory(Arg.Any<string>());
		}
	}
}
