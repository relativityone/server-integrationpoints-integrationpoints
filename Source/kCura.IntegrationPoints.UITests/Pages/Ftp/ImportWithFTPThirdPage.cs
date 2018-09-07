﻿using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Pages.FTP
{
	public class ImportWithFTPThirdPage : ImportThirdPage<ImportFromFTPModel>
	{
		public ImportWithFTPThirdPage(RemoteWebDriver driver) : base(driver)
		{
		}

		public override void SetupModel(ImportFromFTPModel model)
		{
			SetUpCustodianSettingsModel(model.ImportCustodianSettings);
			SetUpSharedSettingsModel(model.SharedImportSettings);
		}
	}
}