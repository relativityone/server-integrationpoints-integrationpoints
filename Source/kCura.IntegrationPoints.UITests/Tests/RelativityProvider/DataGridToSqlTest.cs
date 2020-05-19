using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using System.Threading.Tasks;
using Relativity.Testing.Identification;


namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.EXPORT_TO_RELATIVITY)]
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
			await base.CreateWorkspaceAsync().ConfigureAwait(false);
			SourceContext.EnableDataGrid("Extracted Text");
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
