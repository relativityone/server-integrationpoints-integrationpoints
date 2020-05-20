using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
using kCura.IntegrationPoint.Tests.Core.Models.Import;
using kCura.IntegrationPoint.Tests.Core.Models.Import.FTP;
using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.BrandNew.Import.FTP;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.FTPProvider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.WEB_IMPORT_EXPORT)]
	[Category(TestCategory.IMPORT_FROM_FTP_AND_LDAP)]
	public class ImportFromFtpTest : UiTest
	{
		private IRSAPIService _service;

		private const string _CSV_FILEPATH = "upload/ImportFromFtpTest.csv";

		public ImportFromFtpTest() : base(shouldImportDocuments: false)
		{ }

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Install(SourceContext.WorkspaceId.Value);
			_service = Container.Resolve<IRSAPIService>();
		}

		[IdentifiedTest("780e987a-a791-4048-a2c6-780c191a9998")]
		[RetryOnError]
		[Order(10)]
		public void ImportDocumentsFromFtp()
		{
			// Arrange
			var model = new ImportFromFtpModel($"Import Documents from SFTP ({Now})",
				TransferredObjectConstants.DOCUMENT)
			{
				ConnectionAndFileInfo =
				{
					Host = SharedVariables.FTPConnectionPath,
					Protocol = FtpProtocolType.SFTP,
					Port = 22,
					Username = SharedVariables.FTPUsername,
					Password = SharedVariables.FTPPassword,
					CsvFilepath = _CSV_FILEPATH
				},
                Settings = new SettingsModel
				{
					Overwrite = OverwriteType.AppendOverlay,
					MultiSelectFieldOverlayBehavior = MultiSelectFieldOverlayBehavior.UseFieldSettings,
					CopyNativeFiles = CopyNativeFiles.No,
					UseFolderPathInformation = false,
					CellContainsFileLocation = false,
				}
			};

			// Act
			new ImportDocumentsFromFtpActions(Driver, SourceContext, model).Setup();

			var detailsPage = new IntegrationPointDetailsPage(Driver);
			detailsPage.RunIntegrationPoint();

			// Assert
			new ImportValidator(_service).ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}