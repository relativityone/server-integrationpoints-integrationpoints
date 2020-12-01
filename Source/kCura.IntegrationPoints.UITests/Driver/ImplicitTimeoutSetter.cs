using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public class ImplicitTimeoutSetter : IDisposable
	{
		private readonly IWebDriver _driver;
		private readonly TimeSpan _restoreTo;

		public ImplicitTimeoutSetter(IWebDriver driver, TimeSpan timeout)
		{
			_driver = driver;
			_restoreTo = driver.Manage().Timeouts().ImplicitWait;
			driver.Manage().Timeouts().ImplicitWait = timeout;
		}

		public void Dispose()
		{
			_driver.Manage().Timeouts().ImplicitWait = _restoreTo;
		}
	}
}
