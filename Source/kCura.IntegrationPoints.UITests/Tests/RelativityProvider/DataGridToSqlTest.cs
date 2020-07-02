using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Testing.Identification;


namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync.SavedSearch]
	[Category(TestCategory.RIP_SYNC)]
	public class DataGridToSqlTest : RelativityProviderTestsBase
	{
		private RelativityProviderModel CreateRelativityProviderModelWithNatives()
		{
			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles
			};
			return model;
		}

		protected override async Task CreateWorkspaceAsync()
		{
			await base.CreateWorkspaceAsync().ConfigureAwait(false);
			await SourceContext.EnableDataGridForFieldAsync(TestConstants.FieldNames.EXTRACTED_TEXT).ConfigureAwait(false);
		}

		protected override Task ImportDocumentsAsync()
		{
			return SourceContext.ImportDocumentsWithLargeTextAsync();
		}

		[IdentifiedTest("950ec6af-46a5-42de-9602-685367407032")]
		[Category(TestCategory.SMOKE)]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_01_with_DG()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

		}
	}
}
