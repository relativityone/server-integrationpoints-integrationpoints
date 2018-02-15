using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Threading.Tasks;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Logging;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium.Remote;
using Serilog;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Tests
{
	public abstract class UiTest
	{
		protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(UiTest));
		
		protected TestConfiguration Configuration { get; set; }

		protected TestContext Context { get; set; }

		protected RemoteWebDriver Driver { get; set; }

		protected virtual void ContextSetUp()
		{
		}

		[OneTimeSetUp]
		protected void SetupSuite()
		{
			Configuration = new TestConfiguration()
				.MergeCustomConfigWithAppSettings()
				.SetupConfiguration()
				.LogConfiguration();

			Context = new TestContext();
			Task workspaceCreationTask = Context.CreateWorkspaceAsync();
			Task documentImportTask  = workspaceCreationTask.ContinueWith(async _ => await Context.ImportDocumentsAsync());
			Task integrationPointsInstallationTask = workspaceCreationTask.ContinueWith(async _ => await Context.InstallIntegrationPointsAsync());
			Task contextSetUpTask = Task.Run(() => ContextSetUp());
			Task webDriverCreationTask = CreateDriverAsync();

			Task.WaitAll(documentImportTask, integrationPointsInstallationTask, contextSetUpTask, webDriverCreationTask);
		}

		protected async Task CreateDriverAsync()
		{
			await Task.Run(() => CreateDriver()).ConfigureAwait(false);
		}
		
		protected void CreateDriver()
		{
			ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
			// Otherwise console window appears for chromedriver process
			driverService.HideCommandPromptWindow = true;
			var options = new ChromeOptions();

			// Disables "Save password" popup
			options.AddUserProfilePreference("credentials_enable_service", false);
			options.AddUserProfilePreference("profile.password_manager_enabled", false);
			// Disables "Chrome is being controlled by automated test software." bar
			options.AddArguments("disable-infobars");

			Driver = new ChromeDriver(driverService, options);
			// Long implicit wait as Relativity uses IFrames and is usually quite slow
			Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(SharedVariables.UiImplicitWaitInSec);
			Driver.Manage().Window.Maximize();
			Driver.Url = SharedVariables.ProtocolVersion + "://" + SharedVariables.TargetHost + "/Relativity";
		}
		
		[OneTimeTearDown]
		protected void CloseAndQuitDriver()
		{
			if (!NUnit.Framework.TestContext.CurrentContext.Result.Outcome.Equals(ResultState.Success))
			{
				SaveScreenshot();
			}

			Context.TearDown();

			Driver?.Quit();
		}

		protected GeneralPage EnsureGeneralPageIsOpened()
		{
			var loginPage = new LoginPage(Driver);
			if (loginPage.IsOnLoginPage())
			{
				return loginPage.Login(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			}
			return new GeneralPage(Driver);
		}

		protected void SaveScreenshot()
		{
			if (Driver == null)
			{
				return;
			}
			Screenshot screenshot = ((ITakesScreenshot) Driver).GetScreenshot();
			string testDir = NUnit.Framework.TestContext.CurrentContext.TestDirectory;
			string testName = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
			string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff");
			screenshot.SaveAsFile($@"{testDir}\{timeStamp}_{testName}.png", ScreenshotImageFormat.Png);
		}

		protected string GetExecutorUrl()
		{
			FieldInfo executorField = Driver.GetType().GetField("executor", BindingFlags.NonPublic | BindingFlags.Instance);
			if (executorField == null)
			{
				executorField = Driver.GetType().BaseType.GetField("executor", BindingFlags.NonPublic | BindingFlags.Instance);
			}
			object executor = executorField.GetValue(Driver);
			FieldInfo internalExecutorField = executor.GetType().GetField("internalExecutor", BindingFlags.Instance | BindingFlags.NonPublic);
			object internalExecutor = internalExecutorField.GetValue(executor);
			FieldInfo remoteServerUriField = internalExecutor.GetType().GetField("remoteServerUri", BindingFlags.Instance | BindingFlags.NonPublic);
			var remoteServerUri = remoteServerUriField.GetValue(internalExecutor) as Uri;
			return remoteServerUri.ToString();
		}
	}
}
