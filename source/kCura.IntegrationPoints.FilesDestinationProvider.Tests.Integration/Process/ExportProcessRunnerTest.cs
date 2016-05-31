using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
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
		private ExportSettings _exportSettings;

		#endregion //Fields

		[TestFixtureSetUp]
		public void Init()
		{
			_configSettings = new ConfigSettings();
		    var exportProcessBuilder = new ExportProcessBuilder(Substitute.For<ILoggingMediator>(),
		        Substitute.For<IUserMessageNotification>(), Substitute.For<IUserNotification>(), new UserPasswordCredentialProvider(_configSettings),
                new CaseManagerWrapperFactory(), new SearchManagerWrapperFactory(), new ExporterWrapperFactory(), new ExportFileHelper());
            _instanceUnderTest = new ExportProcessRunner(exportProcessBuilder);
			_workspaceService = new WorkspaceService(_configSettings);
			_exportSettings = CreateExportSettings();

			ImportTestDataToWorkspace(_exportSettings.WorkspaceId);
			CreateOutputFolder();
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			Utility.Directory.Instance.DeleteDirectoryIfExists(_configSettings.DestinationPath, true, false);
			
			if (_exportSettings.WorkspaceId > 0)
			{
				_workspaceService.DeleteWorkspace(_exportSettings.WorkspaceId);
			}
		}

		#region Tests

		[Test]
		[Explicit("Integration Test")]
		public void it_should_export_saved_search()
		{
			// Arrange
			_exportSettings.OverwriteFiles = true;
			_exportSettings.CopyFileFromRepository = true;

			// Act
			_instanceUnderTest.StartWith(_exportSettings);

			// Assert
			ValidateResults(_exportSettings.ExportFilesLocation);
		}

		#endregion //Tests

		#region Methods

		private ExportSettings CreateExportSettings()
		{
			ExportSettings exportSettings = new ExportSettings()
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				ExportFilesLocation = _configSettings.DestinationPath
			};

			exportSettings.WorkspaceId = _workspaceService.CreateWorkspace(_configSettings.WorkspaceName);

			exportSettings.ExportedObjArtifactId = _workspaceService.GetSavedSearchIdBy(_configSettings.SavedSearchArtifactName,
				exportSettings.WorkspaceId);

		    exportSettings.ExportedObjName = _configSettings.SavedSearchArtifactName;

			exportSettings.SelViewFieldIds = _workspaceService.GetFieldIdsBy(_configSettings.SelectedFieldNames, exportSettings.WorkspaceId).ToList();

			return exportSettings;
		}

		private void ImportTestDataToWorkspace(int workspaceId)
		{
			_workspaceService.ImportData(workspaceId, GetDocumentDataTable(), GetImageDataTable());
		}

		private void CreateOutputFolder()
		{
			if (!Utility.Directory.Instance.Exists(_configSettings.DestinationPath, false))
			{
				Utility.Directory.Instance.CreateDirectory(_configSettings.DestinationPath);
			}
		}

		private void ValidateResults(string folder)
		{
			var directory = new DirectoryInfo(folder);
			// Get all directories with NATIVE files
			var nativeDirectories = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories);
			var nativeFileInfos = nativeDirectories.SelectMany(item => item.EnumerateFiles("*", SearchOption.AllDirectories)).ToList();

			var expectedFileNames = GetDocumentDataTable().AsEnumerable().Select(row => row.Field<string>("File Name")).ToList();

			Assert.AreEqual(expectedFileNames.Count, nativeFileInfos.Count(), "Exported Native File count is not like expected!");
			Assert.That(nativeFileInfos.Any(item => expectedFileNames.Exists(name => name == item.Name)));

			var datFileInfo = directory.EnumerateFiles("*.dat", SearchOption.TopDirectoryOnly).FirstOrDefault();
			Assert.That(datFileInfo, Is.Not.Null);
			Assert.That(datFileInfo.Length, Is.GreaterThan(0));
		}

		internal DataTable GetDocumentDataTable()
		{
			DataTable table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("File Name", typeof(string));
			table.Columns.Add("Native File", typeof(string));

			table.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757.htm", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\NATIVES\AMEYERS_0000757.htm"));
			table.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975.pdf", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\NATIVES\AMEYERS_0000975.pdf"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185.xls", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\NATIVES\AMEYERS_0001185.xls"));

			return table;
		}

		internal DataTable GetImageDataTable()
		{
			DataTable table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("Bates Beg", typeof(string));
			table.Columns.Add("File", typeof(string));
			table.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\IMAGES\AMEYERS_0000757.tif"));
			table.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\IMAGES\AMEYERS_0000975.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185_1", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\IMAGES\AMEYERS_0001185.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185_2", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\IMAGES\AMEYERS_0001185_001.tif"));

			return table;
		}

		#endregion Methods
	}
}
