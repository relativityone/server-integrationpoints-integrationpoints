using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class DataGridToSqlTest : RelativityProviderTestsBase
	{
		private RelativityProviderModel CreateRelativityProviderModelWithNatives()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name);
			model.Source = "Saved Search";
			model.RelativityInstance = "This Instance";
			model.DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;
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
