using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium;
using Serilog;
using System;
using System.IO;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ScreenshotSaver
	{
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(ScreenshotSaver));

		public static void SaveScreenshot(IWebDriver driver)
		{
			if (driver == null)
			{
				Log.Warning("Driver is null, screenshot will not be saved.");
				return;
			}
			Screenshot screenshot = ((ITakesScreenshot) driver).GetScreenshot();
			string testDir = NUnit.Framework.TestContext.CurrentContext.TestDirectory;
			string fullTestName = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
			string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff");
			const ScreenshotImageFormat fileType = ScreenshotImageFormat.Png;
			string fileName = $"{timeStamp}_{fullTestName}.{fileType.ToString().ToLower()}";
			string screenshotFullPath = Path.Combine(testDir, fileName);
			Log.Information("Saving screenshot: {ScreenshotFullPath}", screenshotFullPath);
			screenshot.SaveAsFile(screenshotFullPath, fileType);
		}
	}
}
