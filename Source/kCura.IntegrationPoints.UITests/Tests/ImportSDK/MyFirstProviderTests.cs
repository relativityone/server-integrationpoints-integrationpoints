using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Import;
using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Import.MyFirstProvider;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.BrandNew.Import.MyFirstProvider;
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
	public class MyFirstProviderTests : UiTest
	{
		private readonly FileShareHelper _fileshare;
		private string _destinationLocation;

		public MyFirstProviderTests() : base(shouldImportDocuments: false)
		{
			_fileshare = new FileShareHelper(Helper);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			string myFirstProviderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SharedVariables.MyFirstProviderPath);

			RelativityApplicationManager appManager = new RelativityApplicationManager(Helper);
			await appManager.ImportApplicationToLibraryAsync(myFirstProviderPath).ConfigureAwait(false);
			await SourceContext.ApplicationInstallationHelper.InstallMyFirstProviderAsync().ConfigureAwait(false);

			
			await CopyTestDataToFileshareAsync().ConfigureAwait(false);
		}

		private async Task CopyTestDataToFileshareAsync()
		{
			try
			{
				string testData = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataSDK");
				
				string fileSharePath = await _fileshare.GetFilesharePathAsync(SourceContext.GetWorkspaceId()).ConfigureAwait(false);
				_destinationLocation = Path.Combine(fileSharePath, "DataTransfer\\Import");

				await _fileshare.UploadDirectoryAsync(testData, _destinationLocation).ConfigureAwait(false);
			}
			catch (ServiceException ex) when (ex.Message.Contains("already exists"))
			{
				Console.WriteLine("Test data for ImportSDK tests is already copied to file share.");
			}
		}

		[IdentifiedTest("A01C6F51-1784-4988-A577-FCAFB97F4B88")]
		[Category(TestCategory.SMOKE)]
		public void MyFirstProvider_GoldFlow()
		{
			// Arrange
			var model = new ImportDocumentsFromMyFirstProviderModel($"Import Documents from MyFirstProvider ({Now})")
			{
				MyFirstProviderSettings = new MyFirstProviderSettingsModel
				{
					FileLocation= $"{_destinationLocation}\\data.xml"
				},
				
				FieldsMapping = new FieldsMappingModel(
					"Name", "Control Number [Object Identifier]",
					"Text", "Extracted Text [Long Text]"
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
			new ImportDocumentsFromMyFirstProviderActions(Driver, SourceContext, model).Setup();
			
			var detailsPage = new IntegrationPointDetailsPage(Driver);
			detailsPage.RunIntegrationPoint();
			
			// Assert
			new BaseUiValidator().ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}