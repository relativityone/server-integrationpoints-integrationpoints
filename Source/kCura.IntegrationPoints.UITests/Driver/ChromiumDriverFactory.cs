using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium.Remote;
using System;
using System.IO;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromiumDriverFactory
	{
		private static readonly string CHROMIUM_EXE_LOCATION = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SharedVariables.UiBrowserPath);

		public static RemoteWebDriver Create()
		{
			return ChromiumBasedDriverFactory.Create(CHROMIUM_EXE_LOCATION);
		}
	}
}
