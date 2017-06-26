using System;
using System.Diagnostics;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace IntegrationPointsUITests.Pages
{
    public abstract class Page
    {

        protected readonly IWebDriver Driver;

        protected Page(IWebDriver driver)
        {
            Driver = driver;
        }

        public void WaitForPage()
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
            try
            {
                Stopwatch timeWithoutProgressIndicator = Stopwatch.StartNew();
                Stopwatch totalTime = Stopwatch.StartNew();
                while (timeWithoutProgressIndicator.Elapsed.Seconds < SharedVariables.UiWaitForAjaxCallsInSec)
                {
                    if (IsAnyElementVisible(By.Id("progressIndicatorContainer"), By.ClassName("ui-widget-overlay")))
                    {
                        timeWithoutProgressIndicator.Restart();
                    }
                    Thread.Sleep(200);
                    if (totalTime.Elapsed.Minutes > 2)
                    {
                        throw new WebDriverTimeoutException("Progress indicator is visible longer than 2 minutes.");
                    }
                }
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(SharedVariables.UiImplicitWaitInSec);
            }
        }

        public bool IsAnyElementVisible(params By[] bys)
        {
            foreach (By by in bys)
            {
                try
                {
                    if (ExpectedConditions.ElementIsVisible(by)(Driver) != null)
                    {
                        return true;
                    }
                }
                catch (NoSuchElementException)
                {
                    // ignore
                }
            }
            return false;
        }

        public void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

    }
}
