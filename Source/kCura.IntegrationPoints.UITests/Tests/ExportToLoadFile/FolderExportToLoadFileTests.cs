using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class FolderExportToLoadFileTests : UiTest
	{
		private IntegrationPointsAction _integrationPointsAction;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);
		}

		[Test, Order(1)]
		public void FolderExportToLoadFile_TC_ELF_DIR_1()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel("TC_ELF_DIR_1");

			// Step 1
			model.Type = IntegrationPointGeneralModel.IntegrationPointTypeEnum.Export;
			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;
			model.TransferredObject = ExportToLoadFileTransferredObjectConstants.DOCUMENT;

			// Step 2
			model.SourceInformationModel.Source = ExportToLoadFileSourceConstants.FOLDER;
			model.SourceInformationModel.Folder = "One";

			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadfileIntegrationPoint(model);
		}

		private ExportToLoadFileProviderModel CreateExportToLoadFileProviderModel(string name)
		{
			var model = new ExportToLoadFileProviderModel(name, "All Documents");
			return model;
		}
	}
}
