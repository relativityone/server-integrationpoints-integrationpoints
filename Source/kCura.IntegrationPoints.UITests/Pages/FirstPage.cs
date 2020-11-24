using System;
using System.Linq;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class FirstPage : GeneralPage
	{
		public string Name
		{
			get { return NameInput.GetAttribute("value"); }
			set { NameInput.SetTextEx(value, Driver); }
		}

		public string ProfileObject
		{
			get { return ProfileSelect.SelectedOption.Text; }
			set { ProfileSelect.SelectByTextEx(value, Driver); }
		}

		public string Source
		{
			get { return SourceSelect.SelectedOption.Text; }
			set { SourceSelect.SelectByTextEx(value, Driver); }
		}

		public string TransferredObject
		{
			get { return TransferredObjectSelect.SelectedOption.Text; }
			set { TransferredObjectSelect.SelectByTextEx(value, Driver); }
		}

		public string PageMessageText => PageMessage.Text;

		public IWebElement ImportRadioButtonLabel => ImportExportRadioGroup.FindElementsEx(By.TagName("label")).First(e => e.Text == "Import");

		public bool IsExportSelected => ImportExportRadioGroup.FindElementsEx(By.TagName("input")).Last().Selected;

		protected IWebElement NextButton => Driver.FindElementEx(By.Id("next"));

		protected IWebElement SaveButton => Driver.FindElementEx(By.Id("save"));

		protected IWebElement ProfileElement => Driver.FindElementEx(By.Id("apply-profile-selector"));

		protected IWebElement NameInput => Driver.FindElementEx(By.Id("name"));

		protected IWebElement ImportExportRadioGroup => Driver.FindElementEx(By.Id("isExportType"));

		protected IWebElement SourceSelectWebElement => Driver.FindElementEx(By.Id("sourceProvider"));

		protected SelectElement SourceSelect => new SelectElement(SourceSelectWebElement);

		protected IWebElement TransferredObjectWebElement => Driver.FindElementEx(By.Id("destinationRdo"));

		protected SelectElement TransferredObjectSelect => new SelectElement(TransferredObjectWebElement);

		protected IWebElement PageMessage => Driver.FindElementEx(By.ClassName("page-message"));

		protected SelectElement ProfileSelect => new SelectElement(ProfileElement);

		protected FirstPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
		}

		public IntegrationPointDetailsPage SaveIntegrationPoint()
		{
			SaveButton.ClickEx(Driver);
			return new IntegrationPointDetailsPage(Driver);
		}
	}
}