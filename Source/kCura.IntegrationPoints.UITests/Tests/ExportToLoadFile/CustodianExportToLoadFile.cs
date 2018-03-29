using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{

    [TestFixture]
    [Category(TestCategory.SMOKE)]
    public class CustodianExportToLoadFile : UiTest
    {
        private IntegrationPointsAction _integrationPointsAction;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            EnsureGeneralPageIsOpened();
            Context.InstallLegalHold();
            
            _integrationPointsAction = new IntegrationPointsAction(Driver, Context);

        }

        [Test]
        public void CustodianExportToLoadFile_TC_ELF_CUST_1()
        {
            CustodianExportToLoadFileModel model = new CustodianExportToLoadFileModel("TC-ELF-CUST-1");
            model.TransferredObject = "Custodian";
            model.ExportDetails = new CustodianExportToLoadFileDetails
            {
                View = "Custodians - Legal Hold View"
            };
            model.ExportDetails.SelectAllFields = true;
            model.ExportDetails.ExportTextFieldsAsFiles = true;
            model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;

            model.OutputSettings = new ExportToLoadFileOutputSettingsModel
            {
                LoadFileOptions = new ExportToLoadFileLoadFileOptionsModel
                {
                    DataFileFormat = "Comma-separated (.csv)",
                    DataFileEncoding = "Unicode",
                    ExportMultiChoiceAsNested = true,
                    AppendOriginalFileName = true,
                },
                TextOptions = new ExportToLoadFileTextOptionsModel
                {
                    TextFileEncoding =  "Unicode (UTF-8)",
                    TextPrecedence = "Notes",
                    TextSubdirectoryPrefix = "TEXT",
                },
                VolumeAndSubdirectoryOptions = new ExportToLoadFileVolumeAndSubdirectoryModel
                {
                    VolumePrefix = "VOL",
                    VolumeStartNumber = 1,
                    VolumeNumberOfDigits = 4,
                    VolumeMaxSize = 4400,
                    SubdirectoryStartNumber = 1,
                    SubdirectoryNumberOfDigits = 4,
                    SubdirectoryMaxFiles = 500
                }
            };

            IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportCustodianToLoadfileIntegrationPoint(model);
            detailsPage.RunIntegrationPoint();
        }

    }
}
