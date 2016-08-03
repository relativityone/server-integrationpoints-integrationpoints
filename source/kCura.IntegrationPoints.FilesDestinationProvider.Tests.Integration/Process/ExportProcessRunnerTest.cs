using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.Vendor.Castle.MicroKernel.Registration;
using kCura.Vendor.Castle.MicroKernel.Resolvers.SpecializedResolvers;
using kCura.Vendor.Castle.Windsor;
using kCura.WinEDDS.Exporters;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process
{
	public class ExportProcessRunnerTest
	{
		#region Fields

		private readonly string[] _defaultFields = new[] { "Control Number", "File Name", "Issue Designation" };
		private readonly ConfigSettings _configSettings = new ConfigSettings { WorkspaceName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") };

		private ExportProcessRunner _instanceUnderTest;
		private WorkspaceService _workspaceService;
		private DataTable _documents;
		private DataTable _images;

		private static WindsorContainer _windsorContainer;

		#endregion //Fields

		[TestFixtureSetUp]
		public void Init()
		{
			// sets WebApi URL in export library configuration
			kCura.WinEDDS.Config.ProgrammaticServiceURL = _configSettings.WebApiUrl;

			// TODO: ConfigSettings and WorkspaceService have some unhealthy coupling going on...			

			_workspaceService = new WorkspaceService(_configSettings);

			_configSettings.WorkspaceId = _workspaceService.CreateWorkspace(_configSettings.WorkspaceName);
			_configSettings.ExportedObjArtifactId = _workspaceService.GetSavedSearchIdBy(_configSettings.SavedSearchArtifactName, _configSettings.WorkspaceId);

			var fieldsService = _windsorContainer.Resolve<IExportFieldsService>();
			var fields = fieldsService.GetAllExportableFields(_configSettings.WorkspaceId, (int)ArtifactType.Document);

			_configSettings.DefaultFields = fields.Where(x => _defaultFields.Contains(x.DisplayName)).ToArray();

			_configSettings.AdditionalFields = (_configSettings.AdditionalFieldNames.Length > 0) ?
				fields.Where(x => _configSettings.AdditionalFieldNames.Contains(x.DisplayName)).ToArray() :
				fields.Where(x => x.DisplayName.Equals("MD5 Hash")).ToArray();

			_documents = GetDocumentDataTable();
			_images = GetImageDataTable();

			_workspaceService.ImportData(_configSettings.WorkspaceId, _documents, _images);

			CreateOutputFolder(_configSettings.DestinationPath); // root folder for all tests

			var userNotification = _windsorContainer.Resolve<IUserNotification>();
			var exportUserNotification = _windsorContainer.Resolve<IUserMessageNotification>();
			var loggingMediator = _windsorContainer.Resolve<ILoggingMediator>();

			var exportProcessBuilder = new ExportProcessBuilder(
				loggingMediator,
				exportUserNotification,
				userNotification,
				new UserPasswordCredentialProvider(_configSettings),
				new CaseManagerWrapperFactory(),
				new SearchManagerFactory(),
				new ExporterWrapperFactory(),
				new ExportFileBuilder(new DelimitersBuilder(), new VolumeInfoBuilder())
			);

			var exportSettingsBuilder = new ExportSettingsBuilder();

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

		[Explicit("Integration Test")]
		[TestCaseSource(nameof(InvalidFileshareExportTestCaseSource))]
		public void RunInvalidFileshareTestCase(IInvalidFileshareExportTestCase testCase)
		{
			// Arrange
			var settings = testCase.Prepare(CreateExportSettings());

			// Act
			_instanceUnderTest.StartWith(settings);

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

			var settings = new ExportSettings
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				ExportFilesLocation = Path.Combine(_configSettings.DestinationPath, DateTime.UtcNow.ToString("HHmmss_fff")),
				WorkspaceId = _configSettings.WorkspaceId,
				ExportedObjArtifactId = _configSettings.ExportedObjArtifactId,
				ExportedObjName = _configSettings.SavedSearchArtifactName,
				SelViewFieldIds = fieldIds,
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
			table.Columns.Add("Issue Designation", typeof(string));

			table.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757.htm", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\NATIVES\AMEYERS_0000757.htm"), "Level1\\Level2");
			table.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975.pdf", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\NATIVES\AMEYERS_0000975.pdf"), "Level1\\Level2");
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185.xls", Path.Combine(Directory.GetCurrentDirectory(), @"TestData\NATIVES\AMEYERS_0001185.xls"), "Level1\\Level2");

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

		private IEnumerable<IExportTestCase> ExportTestCaseSource()
		{
			InitContainer();
			return _windsorContainer.ResolveAll<IExportTestCase>();
		}

		private IEnumerable<IInvalidFileshareExportTestCase> InvalidFileshareExportTestCaseSource()
		{
			InitContainer();
			return _windsorContainer.ResolveAll<IInvalidFileshareExportTestCase>();
		}

		private void InitContainer()
		{
			if (_windsorContainer != null)
			{
				return;
			}
			_windsorContainer = new WindsorContainer();
			_windsorContainer.Kernel.Resolver.AddSubResolver(new CollectionResolver(_windsorContainer.Kernel));
			_windsorContainer.Register(Classes.FromThisAssembly().IncludeNonPublicTypes().BasedOn<IExportTestCase>().WithServiceAllInterfaces().AllowMultipleMatches());
			_windsorContainer.Register(Classes.FromThisAssembly().IncludeNonPublicTypes().BasedOn<IInvalidFileshareExportTestCase>().WithServiceAllInterfaces().AllowMultipleMatches());

			var exportUserNotification = Substitute.ForPartsOf<ExportUserNotification>();
			_windsorContainer.Register(Component.For<IUserNotification, IUserMessageNotification>().Instance(exportUserNotification).LifestyleSingleton());

			var apiLog = Substitute.For<IAPILog>();
			_windsorContainer.Register(Component.For<IAPILog>().Instance(apiLog).LifestyleSingleton());

			var jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			_windsorContainer.Register(Component.For<IJobHistoryErrorService>().Instance(jobHistoryErrorService).LifestyleSingleton());

			_windsorContainer.Register(Component.For<LoggingMediatorForTestsFactory>().ImplementedBy<LoggingMediatorForTestsFactory>());
			_windsorContainer.Register(Component.For<ILoggingMediator>().UsingFactory((LoggingMediatorForTestsFactory f) => f.Create()).LifestyleSingleton());

			_windsorContainer.Register(Component.For<ConfigSettings>().Instance(_configSettings).LifestyleTransient());
			_windsorContainer.Register(Component.For<ICredentialProvider>().ImplementedBy<UserPasswordCredentialProvider>());
			_windsorContainer.Register(Component.For<IExportFieldsService>().ImplementedBy<ExportFieldsService>().LifestyleTransient());
		}

		#endregion Methods
	}
}
