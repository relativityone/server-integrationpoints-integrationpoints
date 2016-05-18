using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Selenium
	{
		public static IWebDriver WebDriver = new ChromeDriver();

		public static void GoToUrl(string url)
		{
			WebDriver.Navigate().GoToUrl(url);
		}

		public static void LogIntoRelativity(string username, string password)
		{
			string relativityUrl = $"http://{SharedVariables.TargetHost}/relativity";
			GoToUrl(relativityUrl);

			WebDriver.FindElement(By.Id("_email")).SendKeys(username);
			WebDriver.FindElement(By.Id("continue")).Click();

			WebDriver.FindElement(By.Id("_password__password_TextBox")).SendKeys(password);
			WebDriver.FindElement(By.Id("_login")).Click();
		}

		public static void GoToWorkspace(int artifactId)
		{
			string workspaceXpath = $"//a[@href='/Relativity/RedirectHandler.aspx?defaultCasePage=1&AppID={artifactId}&RootFolderID=1003697']";

			WebDriver.SwitchTo().DefaultContent();
			WebDriver.SwitchTo().Frame("ListTemplateFrame");

			WebDriver.FindElement(By.XPath(workspaceXpath)).Click();
		}

		public static void GoToTab(string tabName)
		{
			string tabXpath = $"//a[contains(.,'{tabName}')]";
			WebDriver.FindElement(By.XPath(tabXpath));
		}

		public static void GoToIntegrationPoint(int integrationPointArtifactId )
		{
			string integrationPointXpath = $"//a[contains(@href,'ArtifactID={integrationPointArtifactId})]";
			WebDriver.FindElement(By.XPath(integrationPointXpath)).Click();
		}
	}
}
