using System;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class FirstPage : GeneralPage
	{
		private const string _NAME_INPUT_ID = "name";

		[FindsBy(How = How.Id, Using = "next")]
		protected IWebElement NextButton { get; set; }
		
		protected IWebElement NameInput => Driver.FindElementEx(By.Id(_NAME_INPUT_ID), TimeSpan.FromMinutes(1));

		public string Name
		{
			get { return NameInput.Text; }
			set { NameInput.SetText(value); }
		}

		protected FirstPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
		}

		public bool ValidatePage()
		{
			return IsAnyElementVisible(By.Id(_NAME_INPUT_ID));
		}
	}
}
