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
			set { NameInput.SetText(value); }
		}

		public string ProfileObject
		{
			get { return ProfileSelect.SelectedOption.Text; }
			set { ProfileSelect.SelectByText(value); }
		}

		public string Source
		{
			get { return SourceSelect.SelectedOption.Text; }
			set { SourceSelect.SelectByText(value); }
		}

		public string TransferredObject
		{
			get { return TransferredObjectSelect.SelectedOption.Text; }
			set { TransferredObjectSelect.SelectByText(value); }
		}

		public string PageMessageText => PageMessage.Text;

		public IWebElement ImportRadioButtonLabel => ImportExportRadioGroup.FindElements(By.TagName("label")).First(e => e.Text == "Import");

		public bool IsExportSelected => ImportExportRadioGroup.FindElements(By.TagName("input")).Last().Selected;

		[FindsBy(How = How.Id, Using = "next")]
		protected IWebElement NextButton { get; set; }

		[FindsBy(How = How.Id, Using = "save")]
		protected IWebElement SaveButton { get; set; }

		[FindsBy(How = How.Id, Using = "apply-profile-selector")]
		protected IWebElement ProfileElement { get; set; }

		[FindsBy(How = How.Id, Using = "name")]
		protected IWebElement NameInput { get; set; }

		[FindsBy(How = How.Id, Using = "isExportType")]
		protected IWebElement ImportExportRadioGroup { get; set; }

		[FindsBy(How = How.Id, Using = "sourceProvider")]
		protected IWebElement SourceSelectWebElement { get; set; }

		protected SelectElement SourceSelect => new SelectElement(SourceSelectWebElement);

		[FindsBy(How = How.Id, Using = "destinationRdo")]
		protected IWebElement TransferredObjectWebElement { get; set; }

		protected SelectElement TransferredObjectSelect => new SelectElement(TransferredObjectWebElement);

		[FindsBy(How = How.ClassName, Using = "page-message")]
		protected IWebElement PageMessage { get; set; }

		protected SelectElement ProfileSelect => new SelectElement(ProfileElement);

		protected FirstPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
		}

		public IntegrationPointDetailsPage SaveIntegrationPoint()
		{
			SaveButton.ClickEx();
			return new IntegrationPointDetailsPage(Driver);
		}
	}
}