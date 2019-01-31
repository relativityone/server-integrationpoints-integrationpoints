using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using Castle.Core.Internal;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases;
using kCura.WinEDDS.Core.IO;
using kCura.WinEDDS.Exporters;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using ArtifactType = Relativity.ArtifactType;
using DateTime = System.DateTime;
using Directory = kCura.Utility.Directory;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process
{
	[TestFixture]
	public class ExportProcessRunnerTest
	{
	    #region Fields

		private static readonly ConfigSettings _configSettings = new ConfigSettings {WorkspaceName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")};

		private ExportProcessRunner _instanceUnderTest;
		private WorkspaceService _workspaceService;

		private static WindsorContainer _windsorContainer;

	    #endregion //Fields

		[OneTimeSetUp]
		public void Init()
		{
			// TODO: ConfigSettings and WorkspaceService have some unhealthy coupling going on...

			_workspaceService = new WorkspaceService(new ImportHelper());
		    _configSettings.WorkspaceId = _workspaceService.CreateWorkspace(_configSettings.WorkspaceName);

			var fieldsService = _windsorContainer.Resolve<IExportFieldsService>();
			FieldEntry[] fields = fieldsService.GetAllExportableFields(_configSettings.WorkspaceId, (int) ArtifactType.Document);

			_configSettings.DefaultFields = fields.OrderBy(x => x.DisplayName).ToArray();

			_configSettings.LongTextField = fields.FirstOrDefault(x => x.DisplayName == _configSettings.LongTextFieldName);

			_configSettings.AdditionalFields = new FieldEntry[] {};
		    _configSettings.ExportedObjArtifactId = _workspaceService.CreateSavedSearch(
			    _configSettings.DefaultFields, 
				_configSettings.WorkspaceId, 
			    _configSettings.SavedSearchArtifactName);


			_configSettings.DocumentsTestData = DocumentTestDataBuilder.BuildTestData();

			_workspaceService.ImportData(_configSettings.WorkspaceId, _configSettings.DocumentsTestData);

			_configSettings.ViewId = _workspaceService.GetView(_configSettings.WorkspaceId, _configSettings.ViewName);

			_configSettings.ProductionArtifactId = _workspaceService.CreateAndRunProduction(_configSettings.WorkspaceId, _configSettings.ExportedObjArtifactId,
				_configSettings.ProductionArtifactName);

			CreateOutputFolder(_configSettings.DestinationPath); // root folder for all tests

			var helper = _windsorContainer.Resolve<IHelper>();
			var userNotification = _windsorContainer.Resolve<IUserNotification>();
			var exportUserNotification = _windsorContainer.Resolve<IUserMessageNotification>();
			var loggingMediator = _windsorContainer.Resolve<ICompositeLoggingMediator>();
			var jobStats = Substitute.For<JobStatisticsService>();
			var configMock = Substitute.For<IConfig>();
			configMock.WebApiPath.Returns(SharedVariables.RelativityWebApiUrl);

			var configFactoryMock = Substitute.For<IConfigFactory>();
			configFactoryMock.Create().Returns(configMock);

			IRepositoryFactory repositoryFactory = _windsorContainer.Resolve<IRepositoryFactory>();
			IInstanceSettingRepository instanceSettingRepository =
				_windsorContainer.Resolve<IInstanceSettingRepository>();
			var user = new CurrentUser() {ID = 9};
			IAPILog logger = helper.GetLoggerFactory().GetLogger();
			var exportServiceFactory = new ExportServiceFactory(logger, instanceSettingRepository, repositoryFactory, user);
			
			var exportProcessBuilder = new ExportProcessBuilder(
				configFactoryMock,
				loggingMediator,
				exportUserNotification,
				userNotification,
				new UserPasswordCredentialProvider(),
				_windsorContainer.Resolve<IExtendedExporterFactory>(),
				new ExportFileBuilder(new DelimitersBuilder(), new VolumeInfoBuilder(),
					new ExportedObjectBuilder(new ExportedArtifactNameRepository(_windsorContainer.Resolve<IRSAPIClient>(), _windsorContainer.Resolve<IServiceManagerProvider>()))
					),
				helper,
				jobStats,
				GetJobInfo(),
				new LongPathDirectoryHelper(),
				exportServiceFactory,
				repositoryFactory
			);

			var exportSettingsBuilder = new ExportSettingsBuilder(helper, null);

			_instanceUnderTest = new ExportProcessRunner(exportProcessBuilder, exportSettingsBuilder, helper);
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

		[SmokeTest]
		[TestCaseSource(nameof(ExportTestCaseSource))]
		public void RunStableTestCase(IExportTestCase testCase)
		{
			RunTestCase(testCase);
		}

		[SmokeTest]
		[TestInQuarantine(TestQuarantineState.UnderObservation)]
		[TestCaseSource(nameof(FlakyExportTestCaseSource))]
		public void RunFlakyTestCase(IExportTestCase testCase)
		{
			RunTestCase(testCase);
		}

		public void RunTestCase(IExportTestCase testCase)
		{
			// Arrange
			ExportSettings settings = testCase.Prepare(CreateExportSettings());
			var directory = new DirectoryInfo(settings.ExportFilesLocation);

			CreateOutputFolder(directory.FullName);

			// Act
			_instanceUnderTest.StartWith(settings, JobExtensions.CreateJob());

			// Assert
			testCase.Verify(directory, _configSettings.DocumentsTestData);
		}

        [SmokeTest]
        [TestCaseSource(nameof(InvalidFileshareExportTestCaseSource))]
		public void RunInvalidFileshareTestCase(IInvalidFileshareExportTestCase testCase)
		{
			// Arrange
			ExportSettings settings = testCase.Prepare(CreateExportSettings());

			// Act
			_instanceUnderTest.StartWith(settings, JobExtensions.CreateJob());

			// Assert
			testCase.Verify();
		}

		#region Methods

		private Core.ExportSettings CreateExportSettings()
		{
			var fieldIds = _configSettings.DefaultFields
				.ToDictionary(item => int.Parse(item.FieldIdentifier));

			_configSettings.AdditionalFields.ForEach(item => fieldIds.Add(int.Parse(item.FieldIdentifier), item));

			// Add Long Text Field if not present on the fields list
			if (!fieldIds.ContainsKey(int.Parse(_configSettings.LongTextField.FieldIdentifier)))
			{
				fieldIds.Add(int.Parse(_configSettings.LongTextField.FieldIdentifier), _configSettings.LongTextField);
			}

			var settings = new Core.ExportSettings
			{
				ArtifactTypeId = (int) ArtifactType.Document,
				TypeOfExport = Core.ExportSettings.ExportType.SavedSearch,
				ExportFilesLocation = Path.Combine(_configSettings.DestinationPath, DateTime.UtcNow.ToString("HHmmss_fff")),
				WorkspaceId = _configSettings.WorkspaceId,
				SavedSearchArtifactId = _configSettings.ExportedObjArtifactId,
				SavedSearchName = _configSettings.SavedSearchArtifactName,
				SelViewFieldIds = fieldIds,
				SelectedImageDataFileFormat = Core.ExportSettings.ImageDataFileFormat.None,
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
					},
				SubdirectoryStartNumber = 1,
				VolumeStartNumber = 1
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
			return _windsorContainer
				.ResolveAll<IExportTestCase>()
				.Where(x => !IsFlakyTest(x));
		}

		private static IEnumerable<IExportTestCase> FlakyExportTestCaseSource()
		{
			InitContainer();
			return _windsorContainer
				.ResolveAll<IExportTestCase>()
				.Where(IsFlakyTest);
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

		private IJobInfoFactory GetJobInfo()
		{
			var jobInfoFactory = _windsorContainer.Resolve<IJobInfoFactory>();
			var jobInfo = _windsorContainer.Resolve<IJobInfo>();

			jobInfoFactory.Create(Arg.Any<Job>()).Returns(jobInfo);

			jobInfo.GetStartTimeUtc().Returns(ConfigSettings.JobStart);
			jobInfo.GetName().Returns(ConfigSettings.JobName);

			return jobInfoFactory;
		}

		#endregion Methods

		private static bool IsFlakyTest(IExportTestCase testCase)
		{
			return testCase is ItShouldExportFilesStartingFromGivenRecord
			       || testCase is ItShouldExportFolderAndSubfoldersWithNatives
			       || testCase is ItShouldExportFolderWithNatives
			       || testCase is ItShouldExportImagesAsMultiplePages
			       || testCase is ItShouldExportImagesAsPdfs
			       || testCase is ItShouldExportImagesAsSinglePages
			       || testCase is ItShouldExportOriginalImagesPrecendence
			       || testCase is ItShouldExportProducedImagesPrecedence
			       || testCase is ItShouldExportProductionSetWithImages
			       || testCase is ItShouldExportProductionSetWithNatives
			       || testCase is ItShouldExportSavedSearch
				   || testCase is ItShouldLogJobErrorForNegativeVolumeStartNumber
				   || testCase is ItShouldLogJobErrorForNegativeSubdirectoryStartNumber
				;
		}
	}
}