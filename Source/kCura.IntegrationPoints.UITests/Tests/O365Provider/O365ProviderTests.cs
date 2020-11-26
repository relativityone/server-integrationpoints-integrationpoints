using System.IO;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
using kCura.IntegrationPoint.Tests.Core.Models.Import;
using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Import.O365;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.BrandNew.Import.O365;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.O365Provider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.WebImport.Office365]
	[Category(TestCategory.WEB_IMPORT_EXPORT)]
	public class O365ProviderTests : UiTest
	{
		private readonly FileShareHelper _fileshare;

		public O365ProviderTests() : base(shouldImportDocuments: false)
		{
			_fileshare = new FileShareHelper(Helper);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			RelativityApplicationManager appManager = new RelativityApplicationManager(Helper);
			await appManager.ImportApplicationToLibraryAsync(@"C:\SourceCode\integrationpoints\buildtools\IntegrationPoints.Office365\lib\Office_365_Integration.rap").ConfigureAwait(false);
			await SourceContext.ApplicationInstallationHelper.InstallO365Async().ConfigureAwait(false);

			await CopyFilesToFileshareAsync().ConfigureAwait(false);
		}

		private async Task CopyFilesToFileshareAsync()
		{
			string testData = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataO365");
			string fileSharePath = await _fileshare.GetProcessingSourcePathAsync(SourceContext.GetWorkspaceId()).ConfigureAwait(false);
			await _fileshare.UploadDirectoryAsync(testData, fileSharePath).ConfigureAwait(false);
		}

		[IdentifiedTest("FE811924-3F37-4FCC-B46F-4BAA8B5610EC")]
		public void O365_GoldFlow()
		{
			// Arrange
			var model = new ImportDocumentsFromO365Model($"Import Documents from O365 ({Now})")
			{
				O365Settings = new O365SettingsModel()
				{
					FileName = "Export_load_file_small.csv"
				},
				FieldsMapping = new FieldsMappingModel(
					"Control Number", "Control Number [Object Identifier]",
					"Extracted Text", "Extracted Text [Long Text]",
					"Group Identifier", "Group Identifier [Fixed-Length Text]"
				),
				Settings = new SettingsModel
				{
					Overwrite = OverwriteType.AppendOverlay,
					CopyNativeFiles = CopyNativeFiles.No,
					UseFolderPathInformation = false,
					CellContainsFileLocation = false,
					MultiSelectFieldOverlayBehavior = MultiSelectFieldOverlayBehavior.UseFieldSettings,
				}
			};

			// Act
			new ImportDocumentsFromO365Actions(Driver, SourceContext, model).Setup();

			var detailsPage = new IntegrationPointDetailsPage(Driver);
			detailsPage.RunIntegrationPoint();

			// Assert
			new BaseUiValidator().ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}