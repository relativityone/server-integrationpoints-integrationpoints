using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using NUnit.Framework;
using System;
using System.Net;
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
	using System.Collections.Generic;
	using Data;
	using Validation;

	public abstract class UiTest
	{
		protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(UiTest));

		protected static readonly List<Tuple<string, string>> DefaultFieldsMapping = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Control Number", "Control Number"),
			new Tuple<string, string>("Extracted Text", "Extracted Text"),
			new Tuple<string, string>("Title", "Title"),
			new Tuple<string, string>("Date Created", "Date Created")
		};

		protected static readonly List<Tuple<string, string>> ControlNumberFieldsMapping = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Control Number", "Control Number"),
		};

		protected TestConfiguration Configuration { get; set; }

		protected TestContext Context { get; set; }

		protected RemoteWebDriver Driver { get; set; }

		protected virtual void ContextSetUp()
		{
		}

		[OneTimeSetUp]
		protected void SetupSuite()
		{
			// enable TLS 1.2 for R1 regression environments
			System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			Configuration = new TestConfiguration()
				.MergeCustomConfigWithAppSettings()
				.SetupConfiguration()
				.LogConfiguration();

			Context = new TestContext();
			Task workspaceSetupTask = SetupWorkspaceAsync(); 
			Task webDriverCreationTask = CreateDriverAsync();

			Task.WaitAll(workspaceSetupTask, webDriverCreationTask);
		}

		private async Task SetupWorkspaceAsync()
		{
			await CreateWorkspaceAsync();

			Task installIntegrationPointsTask = Context.InstallIntegrationPointsAsync();

			await ImportDocumentsAsync();

			ContextSetUp();

			await installIntegrationPointsTask;
		}

		protected virtual async Task CreateWorkspaceAsync()
		{
			await Context.CreateWorkspaceAsync();
		}

		protected virtual async Task ImportDocumentsAsync()
		{
			await Context.ImportDocumentsAsync();
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

		protected void WaitForJobToFinishAndValidateCompletedStatus(IntegrationPointDetailsPage detailsPage)
		{
			new BaseUiValidator().ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}
