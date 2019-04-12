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

namespace kCura.IntegrationPoints.UITests.Tests.FTPProvider
{
	[TestFixture]
	[Category(TestCategory.IMPORT_FROM_FTP_AND_LDAP)]
	[Category(TestCategory.NOT_WORKING_ON_REGRESSION_ENVIRONMENT)] // no access to FTP from R1
	public class ImportFromFtpTest : UiTest
	{
		private IRSAPIService _service;

		private const string _CSV_FILEPATH = "All Documents.csv";

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Install(Context.WorkspaceId.Value);
			_service = Container.Resolve<IRSAPIService>();
		}

		[Test]
		[RetryOnError]
		[Order(10)]
		public void ImportDocumentsFromFtp()
		{
			// Arrange
			var model = new ImportFromFtpModel($"Import Documents from FTP ({Now})",
				TransferredObjectConstants.DOCUMENT)
			{
				ConnectionAndFileInfo =
				{
					Host = SharedVariables.FTPConnectionPath,
					Protocol = FtpProtocolType.FTP,
					Port = 21,
					Username = SharedVariables.FTPUsername,
					Password = SharedVariables.FTPPassword,
					CsvFilepath = _CSV_FILEPATH
				},
				FieldsMapping = new FieldsMappingModel(
					"Control Number", "Control Number [Object Identifier]",
					"Extracted Text", "Extracted Text [Long Text]"
				),
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
			new ImportDocumentsFromFtpActions(Driver, Context, model).Setup();

			var detailsPage = new IntegrationPointDetailsPage(Driver);
			detailsPage.RunIntegrationPoint();

			// Assert
			new ImportValidator(_service).ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}