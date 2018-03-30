
using kCura.IntegrationPoint.Tests.Core.Models.Ldap;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ImportWithLdapSecondPage : ImportSecondBasePage<ImportFromLdapModel>
	{
		
		[FindsBy(How = How.Id, Using = "connection-path")]
		protected IWebElement ConnectionPathInput { get; set; }

		[FindsBy(How = How.Id, Using = "connection-filter")]
		protected IWebElement ConnectionFilterInput { get; set; }

		[FindsBy(How = How.Id, Using = "connectionUsername")]
		protected IWebElement UserNameInput { get; set; }

		[FindsBy(How = How.Id, Using = "connectionPassword")]
		protected IWebElement PasswordInput { get; set; }

		[FindsBy(How = How.Id, Using = "s2id_authentication")]
		protected IWebElement AuthenticationSelectWebElement { get; set; }
		
		protected SelectElement AuthenticationModeSelectElement => new SelectElement(AuthenticationSelectWebElement);

		public string AuthenticationMode
		{
			get { return AuthenticationModeSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					AuthenticationModeSelectElement.SelectByText(value);
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
		}

		public override void SetupModel(ImportFromLdapModel model)
		{
			ConnectionPath = model.Source.ConnectionPath;
			ConnectionFilter = model.Source.ObjectFilterString;
			Password = model.Source.Password.ToString();
			UserName = model.Source.Username.ToString();

			AuthenticationMode = model.Source.Authentication.ToString();
		}
	}
}
