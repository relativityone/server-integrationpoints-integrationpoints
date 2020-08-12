using System;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class FirstPage : GeneralPage
	{
		private const string _NAME_INPUT_ID = "name";

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

		public string PageMessageText => PageMessage.Text;

		[FindsBy(How = How.Id, Using = "next")]
		protected IWebElement NextButton { get; set; }

		[FindsBy(How = How.Id, Using = "save")]
		protected IWebElement SaveButton { get; set; }

		[FindsBy(How = How.Id, Using = "apply-profile-selector")]
		protected IWebElement ProfileElement { get; set; }

		[FindsBy(How = How.Id, Using = _NAME_INPUT_ID)]
		protected IWebElement NameInput { get; set; }

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