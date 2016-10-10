using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.Vendor.Castle.Windsor;
using kCura.WinEDDS.Exporters;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using DateTime = System.DateTime;
using Directory = kCura.Utility.Directory;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process
{
	public class ExportProcessRunnerTest
	{
		#region Fields

		private readonly string[] _defaultFields = { "Control Number", "File Name", "Issue Designation" };
		private static readonly ConfigSettings _configSettings = new ConfigSettings { WorkspaceName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") };

		private ExportProcessRunner _instanceUnderTest;
		private WorkspaceService _workspaceService;

		private static WindsorContainer _windsorContainer;

		#endregion //Fields

		[OneTimeSetUp]
		public void Init()
		{
			// TODO: ConfigSettings and WorkspaceService have some unhealthy coupling going on...

			_workspaceService = new WorkspaceService(new ImportHelper(_configSettings));

			_configSettings.WorkspaceId = _workspaceService.CreateWorkspace(_configSettings.WorkspaceName);

			var fieldsService = _windsorContainer.Resolve<IExportFieldsService>();
			var fields = fieldsService.GetAllExportableFields(_configSettings.WorkspaceId, (int) ArtifactType.Document);

			_configSettings.DefaultFields = fields.Where(x => _defaultFields.Contains(x.DisplayName)).ToArray();

			_configSettings.LongTextField = fields.FirstOrDefault(x => x.DisplayName == _configSettings.LongTextFieldName);

			_configSettings.AdditionalFields = _configSettings.AdditionalFieldNames.Length > 0
				? fields.Where(x => _configSettings.AdditionalFieldNames.Contains(x.DisplayName)).ToArray()
				: fields.Where(x => x.DisplayName.Equals("MD5 Hash")).ToArray();

			_configSettings.ExportedObjArtifactId = _workspaceService.CreateSavedSearch(_configSettings.DefaultFields, _configSettings.AdditionalFields,
				_configSettings.WorkspaceId);

			_configSettings.DocumentsTestData = DocumentTestDataBuilder.BuildTestData();

			_workspaceService.ImportData(_configSettings.WorkspaceId, _configSettings.DocumentsTestData);

			_configSettings.ViewId = _workspaceService.GetView(_configSettings.WorkspaceId, _configSettings.ViewName);

			_configSettings.ProductionArtifactId = _workspaceService.CreateProduction(_configSettings.WorkspaceId, _configSettings.ExportedObjArtifactId);

			CreateOutputFolder(_configSettings.DestinationPath); // root folder for all tests

			var userNotification = _windsorContainer.Resolve<IUserNotification>();
			var exportUserNotification = _windsorContainer.Resolve<IUserMessageNotification>();
			var loggingMediator = _windsorContainer.Resolve<ICompositeLoggingMediator>();
			var jobStats = Substitute.For<JobStatisticsService>();
			var configMock = Substitute.For<IConfig>();
			configMock.WebApiPath.Returns(_configSettings.WebApiUrl);

			var instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();
			instanceSettingRepository.GetConfigurationValue(Arg.Any<string>(), Arg.Any<string>()).Returns("false");

			var configFactoryMock = Substitute.For<IConfigFactory>();
			configFactoryMock.Create().Returns(configMock);

			var jobHistoryErrorServiceProvider = _windsorContainer.Resolve<JobHistoryErrorServiceProvider>();

			var exportProcessBuilder = new ExportProcessBuilder(
				configFactoryMock,
				loggingMediator,
				exportUserNotification,
				userNotification,
				new UserPasswordCredentialProvider(_configSettings),
				new CaseManagerFactory(),
				new SearchManagerFactory(),
				new StoppableExporterFactory(jobHistoryErrorServiceProvider, instanceSettingRepository),
				new ExportFileBuilder(new DelimitersBuilder(), new VolumeInfoBuilder()),
				jobStats
			);

			var exportSettingsBuilder = new ExportSettingsBuilder();

			_instanceUnderTest = new ExportProcessRunner(exportProcessBuilder, exportSettingsBuilder);
		}

		[OneTimeTearDown]
		public void CleanUp()
		{
			Directory.Instance.DeleteDirectoryIfExists(_configSettings.DestinationPath, true, false);

			if (_configSettings.WorkspaceId > 0)
			{
				_workspaceService.DeleteWorkspace(_configSettings.WorkspaceId);
			}
		}

		[Explicit("Integration Test")]
		[TestCaseSource(nameof(ExportTestCaseSource))]
		public void RunTestCase(IExportTestCase testCase)
		{
			// Arrange
			var settings = testCase.Prepare(CreateExportSettings());
			var directory = new DirectoryInfo(settings.ExportFilesLocation);

			CreateOutputFolder(directory.FullName);

			// Act
			_instanceUnderTest.StartWith(settings, JobExtensions.CreateJob());

			// Assert
			testCase.Verify(directory, _configSettings.DocumentsTestData);
		}

		[Explicit("Integration Test")]
		[TestCaseSource(nameof(InvalidFileshareExportTestCaseSource))]
		public void RunInvalidFileshareTestCase(IInvalidFileshareExportTestCase testCase)
		{
			// Arrange
			var settings = testCase.Prepare(CreateExportSettings());

			// Act
			_instanceUnderTest.StartWith(settings, JobExtensions.CreateJob());

			// Assert
			testCase.Verify();
		}

		#region Methods

		private ExportSettings CreateExportSettings()
		{
			var fieldIds = _configSettings.DefaultFields
				.Select(x => int.Parse(x.FieldIdentifier))
				.ToList();

			fieldIds.AddRange(_configSettings.AdditionalFields.Select(x => int.Parse(x.FieldIdentifier)));

			// Add Long Text Field
			fieldIds.Add(int.Parse(_configSettings.LongTextField.FieldIdentifier));

			var settings = new ExportSettings
			{
				ArtifactTypeId = (int) ArtifactType.Document,
				TypeOfExport = ExportSettings.ExportType.SavedSearch,
				ExportFilesLocation = Path.Combine(_configSettings.DestinationPath, DateTime.UtcNow.ToString("HHmmss_fff")),
				WorkspaceId = _configSettings.WorkspaceId,
				SavedSearchArtifactId = _configSettings.ExportedObjArtifactId,
				SavedSearchName = _configSettings.SavedSearchArtifactName,
				SelViewFieldIds = fieldIds,
				SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.None,
				TextPrecedenceFieldsIds = new List<int> {int.Parse(_configSettings.LongTextField.FieldIdentifier)},
				DataFileEncoding = Encoding.Unicode,
				VolumeMaxSize = 650,
				ImagePrecedence =
					new[]
					{
						new ProductionDTO
						{
							ArtifactID = _configSettings.ProductionArtifactId.ToString(),
							DisplayName = "Production"
						}
					}
			};

			return settings;
		}

		private static void CreateOutputFolder(string path)
		{
			if (!Directory.Instance.Exists(path, false))
			{
				Directory.Instance.CreateDirectory(path);
			}
		}

		private static IEnumerable<IExportTestCase> ExportTestCaseSource()
		{
			InitContainer();
			return _windsorContainer.ResolveAll<IExportTestCase>();
		}

		private static IEnumerable<IInvalidFileshareExportTestCase> InvalidFileshareExportTestCaseSource()
		{
			InitContainer();
			return _windsorContainer.ResolveAll<IInvalidFileshareExportTestCase>();
		}

		private static void InitContainer()
		{
			if (_windsorContainer == null)
			{
				_windsorContainer = ContainerInstaller.CreateContainer(_configSettings);
			}
		}

		#endregion Methods
	}
}