using OpenQA.Selenium.Remote;
using System;
using System.IO;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromiumDriverFactory
	{
		private static readonly string CHROMIUM_EXE_LOCATION = Path.GetFullPath(
			Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\packages\testpackages\Relativity.Chromium.Portable\tools\Chrome.exe"));

		public static RemoteWebDriver Create()
		{
			return ChromiumBasedDriverFactory.Create(CHROMIUM_EXE_LOCATION);
		}
	}
}
