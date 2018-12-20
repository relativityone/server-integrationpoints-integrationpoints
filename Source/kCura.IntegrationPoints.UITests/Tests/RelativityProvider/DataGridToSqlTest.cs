using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.DATA_GRID_RELATED)]
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
			await base.CreateWorkspaceAsync();
			Context.EnableDataGrid("Extracted Text");
		}

		protected override async Task ImportDocumentsAsync()
		{
			await Context.ImportDocumentsWithLargeTextAsync();
		}

		[Test]
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
