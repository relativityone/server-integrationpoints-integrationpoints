using System;
using System.IO;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Import;
using kCura.IntegrationPoint.Tests.Core.Models.Import.JsonLoader;
using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.ImportSdk
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.WebImport.ImportSdk]
	[Category(TestCategory.RIP_OLD)]
	public class JsonLoaderProviderTests : UiTest
	{
		private readonly FileShareHelper _fileshare;
		private string destinationLocation;

		public JsonLoaderProviderTests() : base(shouldImportDocuments: false)
		{
			_fileshare = new FileShareHelper(Helper);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			RelativityApplicationManager appManager = new RelativityApplicationManager(Helper);
			await appManager.ImportApplicationToLibraryAsync(SharedVariables.JsonLoaderPath).ConfigureAwait(false);
			await SourceContext.ApplicationInstallationHelper.InstallJsonLoaderAsync().ConfigureAwait(false);

			
			await CopyTestDataToFileshareAsync().ConfigureAwait(false);
		}

		private async Task CopyTestDataToFileshareAsync()
		{
			try
			{
				string testData = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataSDK");
				
				string fileSharePath = await _fileshare.GetFilesharePathAsync(SourceContext.GetWorkspaceId()).ConfigureAwait(false);
				destinationLocation = Path.Combine(fileSharePath, "DataTransfer\\Import");

				await _fileshare.UploadDirectoryAsync(testData, destinationLocation).ConfigureAwait(false);
			}
			catch (ServiceException ex) when (ex.Message.Contains("already exists"))
			{
				Console.WriteLine("Test data for ImportSDK tests is already copied to file share.");
			}
		}

		[IdentifiedTest("FBDC424B-2FB4-49F5-800A-A98874136840")]
		[Category(TestCategory.SMOKE)]
		public void JsonLoader_GoldFlow()
		{
			// Arrange
			var model = new ImportDocumentsFromJsonLoaderModel($"Import Documents from JsonLoader ({Now})")
			{
				JsonLoaderSettings = new JsonLoaderSettingsModel()
				{
					DataLocation = $"{destinationLocation}\\data.json",
					FieldLocation = $"{destinationLocation}\\fields.json",
					UniqueIdentifier = "Name"
				},
				
				FieldsMapping = new FieldsMappingModel(
					"ID", "Name [Object Identifier]",
					"Filename", "Sample Text Field [Fixed-Length Text]"
				),
				Settings = new SettingsModel
				{
					Overwrite = OverwriteType.AppendOnly,
					CopyNativeFiles = CopyNativeFiles.No,
					UseFolderPathInformation = false,
					CellContainsFileLocation = false,
					MultiSelectFieldOverlayBehavior = MultiSelectFieldOverlayBehavior.UseFieldSettings
				}
			};

			// Act
			new ImportDocumentsFromJsonLoaderActions(Driver, SourceContext, model).Setup();
			
			var detailsPage = new IntegrationPointDetailsPage(Driver);
			detailsPage.RunIntegrationPoint();
			
			// Assert
			new BaseUiValidator().ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}