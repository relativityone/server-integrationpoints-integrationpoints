using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium.Remote;
using System;
using System.IO;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromiumDriverFactory
	{
		private static readonly string _chromium_exe_location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SharedVariables.UiBrowserPath);

		public static RemoteWebDriver Create()
		{
			return ChromiumBasedDriverFactory.Create(_chromium_exe_location);
		}
	}
}
