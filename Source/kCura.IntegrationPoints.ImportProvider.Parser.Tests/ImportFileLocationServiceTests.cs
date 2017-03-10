using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoint.Tests.Core;

using NUnit.Framework;
using NSubstitute;

using SystemInterface.IO;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	[TestFixture, Category("ImportProvider")]
	public class ImportFileLocationServiceTests : TestBase
	{
		private const int _IP_ARTIFACT_ID = 1004242;
		private const string _LOAD_FILE_PATH = @"DataTransfer\Import\example-load-file.csv";
		private const string _WORKSPACE_ROOT_LOCATION = @"\\example.host.name\fileshare\EDDS-example-app-id";
		private const string _IP_NAME = @"Example-IP-Name";
		private const string _ERROR_FILE_LOCATION =
			@"\\example.host.name\fileshare\EDDS-example-app-id\DataTransfer\Import\Error_Files\Example-IP-Name-1004242-Error_file.csv";
		private const string _LOAD_FILE_LOCATION =
			@"\\example.host.name\fileshare\EDDS-example-app-id\DataTransfer\Import\example-load-file.csv";

		IIntegrationPointService _integrationPointReader;
		IDataTransferLocationService _locationService;
		ISerializer _serializer;
		IDirectory _directoryHelper;

		Data.IntegrationPoint _ip;
		ImportProviderSettings _providerSettings;
		ImportSettings _importApiSettings;

		[SetUp]
		public override void SetUp()
		{
			_ip = new Data.IntegrationPoint();
			_providerSettings = new ImportProviderSettings();
			_importApiSettings = new ImportSettings();

			_ip.Name = _IP_NAME;
			_ip.SourceConfiguration = string.Empty;
			_ip.DestinationConfiguration = string.Empty;
			_importApiSettings.CaseArtifactId = -1;
			_providerSettings.LoadFile = _LOAD_FILE_PATH;

			_integrationPointReader = Substitute.For<IIntegrationPointService>();
			_locationService = Substitute.For<IDataTransferLocationService>();
			_serializer = Substitute.For<ISerializer>();
			_directoryHelper = Substitute.For<IDirectory>();

			_integrationPointReader.GetRdo(Arg.Any<int>()).ReturnsForAnyArgs(_ip);
			_serializer.Deserialize<ImportProviderSettings>(Arg.Any<string>()).ReturnsForAnyArgs(_providerSettings);
			_serializer.Deserialize<ImportSettings>(Arg.Any<string>()).ReturnsForAnyArgs(_importApiSettings);
			_locationService.GetWorkspaceFileLocationRootPath(Arg.Any<int>()).ReturnsForAnyArgs(_WORKSPACE_ROOT_LOCATION);
		}

		[Test]
		public void ItShouldReturnTheCorrectErrorFilePath()
		{
			//Arrange
			_directoryHelper.Exists(Arg.Any<string>()).ReturnsForAnyArgs(true);
			ImportFileLocationService locationService = new ImportFileLocationService(_integrationPointReader,
				_locationService,
				_serializer,
				_directoryHelper);

			//Act
			string generatedErrorFilePath = locationService.ErrorFilePath(_IP_ARTIFACT_ID);

			//Assert
			Assert.AreEqual(_ERROR_FILE_LOCATION, generatedErrorFilePath);
		}

		[Test]
		public void ItShouldReturnTheCorrectLoadFileFullPath()
		{
			//Arrange
			ImportFileLocationService locationService = new ImportFileLocationService(_integrationPointReader,
				_locationService,
				_serializer,
				_directoryHelper);

			//Act
			string generatedLoadFilePath = locationService.LoadFileFullPath(_IP_ARTIFACT_ID);

			//Assert
			Assert.AreEqual(_LOAD_FILE_LOCATION, generatedLoadFilePath);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void ItShouldCreateDirectoryIfNecessary(bool directoryExists)
		{
			//Arrange
			_directoryHelper.Exists(Arg.Any<string>()).ReturnsForAnyArgs(directoryExists);
			ImportFileLocationService locationService = new ImportFileLocationService(_integrationPointReader,
				_locationService,
				_serializer,
				_directoryHelper);

			//Act
			locationService.ErrorFilePath(_IP_ARTIFACT_ID);

			//Assert
			_directoryHelper.Received(directoryExists ? 0 : 1).CreateDirectory(Arg.Any<string>());
		}
	}
}
