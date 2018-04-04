using System.Collections.Generic;
using System.Security;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Actions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using kCura.Vendor.Castle.Core.Internal;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.FTPProvider
{
	public class ImportFromFTPTest : UiTest
	{
		private IRSAPIService _service;
		private IntegrationPointsImportFTPAction _integrationPointsAction;
		private const string _CSV_FILEPATH = "TestFtpCustodianImport.csv";
		private const string _UNIQUE_ID = "UniqueID";

		protected override bool InstallLegalHoldApp => true;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsImportFTPAction(Driver, Context);
			Install(Context.WorkspaceId.Value);
			_service = Container.Resolve<IRSAPIService>();
		}

		[Test, Order(1)]
		public void CustodianImportFromFTP_TC_IFTP_CUS_1()
		{
			// Arrange
			var model = new ImportFromFTPModel("TC_IFTP_CUS_1", ExportToLoadFileTransferredObjectConstants.CUSTODIAN);

			// Step 2
			model.ConnectionAndFileInfo.Host = SharedVariables.FTPConnectionPath;
			model.ConnectionAndFileInfo.Protocol = FTPProtocolType.FTP;
			model.ConnectionAndFileInfo.Port = "21";

			model.ConnectionAndFileInfo.Username = new SecureString();
			SharedVariables.FTPUsername.ForEach(c => model.ConnectionAndFileInfo.Username.AppendChar(c));

			model.ConnectionAndFileInfo.Password = new SecureString();
			SharedVariables.FTPPassword.ForEach(c => model.ConnectionAndFileInfo.Password.AppendChar(c));

			model.ConnectionAndFileInfo.CSVFilepath = _CSV_FILEPATH;

			// Step 3
			model.SharedImportSettings.MapFieldsAutomatically = true;
			model.SharedImportSettings.Overwrite = OverwriteType.AppendOnly;
			model.ImportCustodianSettings.UniqueIdentifier = _UNIQUE_ID;
			model.ImportCustodianSettings.CustodianManagerContainsLink = true;

			var validator = new ImportValidator(_service);

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewImportFromFTPIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			var dict = new Dictionary<string, bool>
			{
				{"john.doe@test.com", true},
				{"jane.kane@test.com", false}
			};
			validator.ValidateCustodians(dict);
		}
	}
}