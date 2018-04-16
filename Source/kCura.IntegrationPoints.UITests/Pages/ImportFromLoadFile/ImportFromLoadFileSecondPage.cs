using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages.ImportFromLoadFile
{
	public class ImportFromLoadFileSecondPage : ImportSecondBasePage<ImportFromLoadFileModel>
	{
		public ImportFromLoadFileSecondPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
		}

		public ImportFromLoadFileSecondPageLoadFileSettings LoadFileSettings { get; set; }
		// TODO

		public override void SetupModel(ImportFromLoadFileModel model)
		{
			LoadFileSettings.SetupModel(model);
			// TODO
		}
	}
}
