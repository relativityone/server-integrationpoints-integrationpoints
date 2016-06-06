using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoint.Tests.Core
{
    using System;
    using System.Collections.ObjectModel;

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
            ReadOnlyCollection<IWebElement> webElementCollection = WebDriver.FindElements(By.Id("horizontal-tabstrip"));
            IWebElement navigationList = webElementCollection[0].FindElement(By.XPath("//ul[@class='nav navbar-nav']"));
            ReadOnlyCollection<IWebElement> listElements = navigationList.FindElements(By.TagName("li"));
		    foreach (IWebElement listElement in listElements)
		    {
                ReadOnlyCollection<IWebElement> anchorCollectoin = listElement.FindElements(By.TagName("a"));

                foreach (IWebElement anchor in anchorCollectoin)
		        {
		            if (anchor.Text.Equals(tabName))
		            {
                        anchor.Click();
		                return;
		            }
		        }
		    }
		}

		public static void GoToObjectInstance(int workspaceArtifactId, int integrationPointArtifactId, int artifactTypeId)
		{
			string integrationPointUrl = $"http://{SharedVariables.TargetHost}/Relativity/Case/Mask/View.aspx?AppID={workspaceArtifactId}&ArtifactID={integrationPointArtifactId}&ArtifactTypeID={artifactTypeId}";
			WebDriver.Navigate().GoToUrl(integrationPointUrl);
		}

        public static bool PageShouldContain(string message)
        {
            return WebDriver.PageSource.Contains(message);
        }

	    public static void WaitUntilIdExists(string id, int timeSeconds)
	    {
	        WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(timeSeconds));
	        wait.Until(ExpectedConditions.ElementExists(By.Id(id)));
	    }

        public static void WaitUntilIdIsClickable(string id, int timeSeconds)
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(timeSeconds));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(id)));
        }

        public static void WaitUntilXpathIsClickable(string xpath, int timeSeconds)
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(timeSeconds));
            wait.Until(
                ExpectedConditions.ElementToBeClickable(By.XPath(xpath)));
        }

        public static void WaitUntilXpathExists(string xpath, int timeSeconds)
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(timeSeconds));
            wait.Until(ExpectedConditions.ElementExists(By.XPath(xpath)));
        }

        public static void WaitUntilXpathVisible(string xpath, int timeSeconds)
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(timeSeconds));
            wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath(xpath)));
        }

        public static void SelectFromDropdownList(string dropdownId, string value)
        {
            IWebElement dropDown = Selenium.WebDriver.FindElement(By.Id(dropdownId));
            SelectElement selectValue = new SelectElement(dropDown);
            selectValue.SelectByText(value);
        }


    }
}
