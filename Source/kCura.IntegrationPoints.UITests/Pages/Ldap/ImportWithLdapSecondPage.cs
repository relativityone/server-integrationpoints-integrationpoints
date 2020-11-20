using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models.Import.Ldap;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.Utility;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages.Ldap
{
	public class ImportWithLdapSecondPage : ImportSecondBasePage<ImportFromLdapModel>
	{
		protected IWebElement ConnectionPathInput => Driver.FindElementEx(By.Id("connectionPath"));

		protected IWebElement ConnectionFilterInput => Driver.FindElementEx(By.Id("connectionFilter"));

		protected IWebElement UserNameInput => Driver.FindElementEx(By.Id("connectionUsername"));

		protected IWebElement PasswordInput => Driver.FindElementEx(By.Id("connectionPassword"));

		protected IWebElement AuthenticationSelectWebElement => Driver.FindElementEx(By.Id("authentication"));
		
		protected SelectElement AuthenticationModeSelectElement => new SelectElement(AuthenticationSelectWebElement);

		public string AuthenticationMode
		{
			get { return AuthenticationModeSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					AuthenticationModeSelectElement.SelectByTextEx(value, Driver);
				}
			}
		}

		public string ConnectionPath
		{
			get { return ConnectionPathInput.Text; }
			set
			{
				if (value != null)
				{
					SetInputText(ConnectionPathInput, value);
				}
			}
		}

		public string ConnectionFilter
		{
			get { return ConnectionFilterInput.Text; }
			set
			{
				if (value != null)
				{
					SetInputText(ConnectionFilterInput, value);
				}
			}
		}

		public string UserName
		{
			get { return UserNameInput.Text; }
			set
			{
				if (value != null)
				{
					SetInputText(UserNameInput, value);
				}
			}
		}

		public string Password
		{
			get { return PasswordInput.Text; }
			set
			{
				if (value != null)
				{
					SetInputText(PasswordInput, value);
				}
			}
		}

		public ImportWithLdapSecondPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
		}

		public override void SetupModel(ImportFromLdapModel model)
		{
			ConnectionFilter = model.Source.ObjectFilterString;
			ConnectionPath = model.Source.ConnectionPath;
			AuthenticationMode = model.Source.Authentication.GetDescription();
			Password = model.Source.Password.ToPlainString();
			UserName = model.Source.Username.ToPlainString();
		}
	}
}
