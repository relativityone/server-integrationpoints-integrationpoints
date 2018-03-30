﻿using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Actions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.FTPProvider
{
	public class ImportFromFTPTest : UiTest
	{
		private const string _CSV_FILEPATH = "TestFtpCustodianImport.csv";
		private const string _UNIQUE_ID = "UniqueID";

		private IntegrationPointsImportFTPAction _integrationPointsAction;

		protected override bool InstallLegalHoldApp => true;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsImportFTPAction(Driver, Context);
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
			model.ConnectionAndFileInfo.Username = SharedVariables.FTPUsername;
			model.ConnectionAndFileInfo.Password = SharedVariables.FTPPassword;
			model.ConnectionAndFileInfo.CSVFilepath = _CSV_FILEPATH;

			// Step 3
			model.SharedImportSettings.MapFieldsAutomatically = true;
			model.SharedImportSettings.Overwrite = OverwriteType.AppendOnly;
			model.ImportCustodianSettings.UniqueIdentifier = _UNIQUE_ID;
			model.ImportCustodianSettings.CustodianManagerContainsLink = true;



			_integrationPointsAction.CreateNewImportFromFTPIntegrationPoint(model);
		}
	}
}