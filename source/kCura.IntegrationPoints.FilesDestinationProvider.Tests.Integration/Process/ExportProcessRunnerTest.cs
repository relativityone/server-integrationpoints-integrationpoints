using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.WinEDDS.Exporters;
using NSubstitute;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process
{
	public class ExportProcessRunnerTest
	{
		#region Fields

		private ExportProcessRunner _instanceUnderTest;
		private ConfigSettings _configSettings;
		private WorkspaceService _workspaceService;
		private DataTable _documents;
		private DataTable _images;

		#endregion //Fields

		[TestFixtureSetUp]
		public void Init()
		{
			// TODO: ConfigSettings and WorkspaceService have some unhealthy coupling going on...

		    _configSettings = new ConfigSettings
		    {
                WorkspaceName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")
		    };

			_workspaceService = new WorkspaceService(_configSettings);

			_configSettings.WorkspaceId = _workspaceService.CreateWorkspace(_configSettings.WorkspaceName);
			_configSettings.ExportedObjArtifactId = _workspaceService.GetSavedSearchIdBy(_configSettings.SavedSearchArtifactName, _configSettings.WorkspaceId);
			_configSettings.SelViewFieldIds = _workspaceService.GetFieldIdsBy(_configSettings.SelectedFieldNames, _configSettings.WorkspaceId).ToList();

			_documents = GetDocumentDataTable();
			_images = GetImageDataTable();

			_workspaceService.ImportData(_configSettings.WorkspaceId, _documents, _images);

			CreateOutputFolder(_configSettings.DestinationPath); // root folder for all tests

			var userNotification = Substitute.For<IUserNotification>();
			userNotification.AlertWarningSkippable(Arg.Any<string>()).Returns(true);

			var exportProcessBuilder = new ExportProcessBuilder(
				Substitute.For<ILoggingMediator>(),
				Substitute.For<IUserMessageNotification>(),
				userNotification,
				new UserPasswordCredentialProvider(_configSettings),
				new CaseManagerWrapperFactory(),
				new SearchManagerFactory(),
				new ExporterWrapperFactory(),
				new ExportFileBuilder(new DelimitersBuilder(), new VolumeInfoBuilder())
			);

			var exportSettingsBuilder= new ExportSettingsBuilder(); 

			_instanceUnderTest = new ExportProcessRunner(exportProcessBuilder, exportSettingsBuilder);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			Utility.Directory.Instance.DeleteDirectoryIfExists(_configSettings.DestinationPath, true, false);

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
			_instanceUnderTest.StartWith(settings);

			// Assert
			testCase.Verify(directory, _documents, _images);
		}

		#region Methods

		private ExportSettings CreateExportSettings()
		{
			var settings = new ExportSettings
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				ExportFilesLocation = Path.Combine(_configSettings.DestinationPath, DateTime.UtcNow.ToString("HHmmss_fff")),
				WorkspaceId = _configSettings.WorkspaceId,
				ExportedObjArtifactId = _configSettings.ExportedObjArtifactId,
				ExportedObjName = _configSettings.SavedSearchArtifactName,
				SelViewFieldIds = _configSettings.SelViewFieldIds,
				DataFileEncoding = Encoding.Unicode,
				VolumeMaxSize = 650
			};

			return settings;
		}

		private static void CreateOutputFolder(string path)
		{
			if (!Utility.Directory.Instance.Exists(path, false))
			{
				Utility.Directory.Instance.CreateDirectory(path);
			}
		}

		private static DataTable GetDocumentDataTable()
		{
			var table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("File Name", typeof(string));
			table.Columns.Add("Native File", typeof(string));

			table.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757.htm", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\NATIVES\AMEYERS_0000757.htm"));
			table.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975.pdf", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\NATIVES\AMEYERS_0000975.pdf"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185.xls", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\NATIVES\AMEYERS_0001185.xls"));

			return table;
		}

		private static DataTable GetImageDataTable()
		{
			var table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("Bates Beg", typeof(string));
			table.Columns.Add("File", typeof(string));

			table.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\IMAGES\AMEYERS_0000757.tif"));
			table.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\IMAGES\AMEYERS_0000975.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\IMAGES\AMEYERS_0001185.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185_001", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\IMAGES\AMEYERS_0001185_001.tif"));

			return table;
		}

		private static IEnumerable<IExportTestCase> ExportTestCaseSource()
		{
			var cases = System.Reflection.Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(t => t.GetInterfaces().Contains(typeof(IExportTestCase)) && !t.IsAbstract)
				.Select(Activator.CreateInstance)
				.Cast<IExportTestCase>();

			return cases;
		}

		#endregion Methods
	}
}
