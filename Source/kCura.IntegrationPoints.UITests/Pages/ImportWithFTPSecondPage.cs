using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ImportWithFTPSecondPage : GeneralPage
	{
		[FindsBy(How = How.Id, Using = "next")]
		protected IWebElement NextButton { get; set; }

		[FindsBy(How = How.Id, Using = "host")]
		protected IWebElement HostInput { get; set; }

		[FindsBy(How = How.Id, Using = "s2id_protocol")]
		protected IWebElement ProtocolSelectWebElement { get; set; }

		protected SelectElement ProtocolSelectElement => new SelectElement(ProtocolSelectWebElement);

		[FindsBy(How = How.Id, Using = "port")]
		protected IWebElement PortInput { get; set; }

		[FindsBy(How = How.Id, Using = "username")]
		protected IWebElement UsernameInput { get; set; }

		[FindsBy(How = How.Id, Using = "password")]
		protected IWebElement PasswordInput { get; set; }

		[FindsBy(How = How.Id, Using = "filename_prefix")]
		protected IWebElement CSVFilepathInput { get; set; }

		public ImportWithFTPSecondPage(RemoteWebDriver driver) : base(driver)
		{
		}

		public string Host
		{
			get { return HostInput.Text; }
			set { SetInputText(HostInput, value); }
		}

		public string Protocol
		{
			get { return ProtocolSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					ProtocolSelectElement.SelectByText(value);
				}
			}
		}

		public string Port
		{
			get { return PortInput.Text; }
			set { SetInputText(PortInput, value); }
		}

		public string Username
		{
			get { return UsernameInput.Text; }
			set { SetInputText(UsernameInput, value); }
		}

		public string Password
		{
			get { return PasswordInput.Text; }
			set { SetInputText(PasswordInput, value); }
		}

		public string CSVFilepath
		{
			get { return CSVFilepathInput.Text; }
			set { SetInputText(CSVFilepathInput, value); }
		}

		protected void SetInputText(IWebElement element, string text)
		{
			element.Clear();
			element.SendKeys(text);
		}

		public ImportThirdPage GoToNextPage()
		{
			WaitForPage();
			NextButton.Click();
			return new ImportThirdPage(Driver);
		}
	}
}
