using IntegrationPointsUITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using TestHelper = kCura.IntegrationPoint.Tests.Core.TestHelpers.TestHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Relativity.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework.Interfaces;

namespace IntegrationPointsUITests.Tests
{
	public abstract class UiTest
	{
		private Lazy<ITestHelper> _helper;
		private const string _TEMPALTE_WKSP_NAME = "Relativity Starter Template";
		private const int _ADMIN_USER_ID = 9;

		protected string TestTimeStamp { get; set; }

		protected int WorkspaceId { get; set; } = int.MinValue;
		protected string WorkspaceName { get; set; }

		protected int GroupId { get; set; }
		protected int UserId { get; set; }

		protected IWebDriver Driver { get; set; }

		public ITestHelper Helper => _helper.Value;

		[OneTimeSetUp]
		protected void SetupSuite()
		{
			TestTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
			SetupConfiguration();
			CreateWorkspace();
			SetupUser();
			ImportDocuments();
			InstallIntegrationPoints();
			CreateDriver();
		}

		protected void SetupConfiguration()
		{
			kCura.Data.RowDataGateway.Config.MockConfigurationValue("LongRunningQueryTimeout", 100);
			string connString = string.Format(ConfigurationManager.AppSettings["connectionStringEDDS"],
				SharedVariables.TargetDbHost, SharedVariables.DatabaseUserId, SharedVariables.DatabasePassword);
			kCura.Config.Config.SetConnectionString(connString);

			_helper = new Lazy<ITestHelper>(() => new TestHelper());

			Relativity.Data.Config.InjectConfigSettings(new Dictionary<string, object>
			{
				{"connectionString", SharedVariables.EddsConnectionString}
			});
		}

		protected void CreateWorkspace()
		{
			WorkspaceName = $"Test Workspace {TestTimeStamp}";

			try
			{
				WorkspaceId = Workspace.CreateWorkspace($"Test Workspace {TestTimeStamp}", _TEMPALTE_WKSP_NAME);
			}
			catch (System.Exception ex)
			{
				Console.WriteLine($@"Cannot create workspace. Check if Relativity works correctly (services, ...). Exception: {ex}.");
				throw;
			}
		}

		protected void SetupUser()
		{
			GroupId = Group.CreateGroup($"TestGroup_{TestTimeStamp}");
			Group.AddGroupToWorkspace(WorkspaceId, GroupId);
			
			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
			{
				var factory = new ClaimsPrincipalFactory();
				return factory.CreateClaimsPrincipal2(_ADMIN_USER_ID, Helper);
			};

			UserModel userModel = User.CreateUser("UI", $"Test_User_{TestTimeStamp}", $"UI_Test_User_{TestTimeStamp}@relativity.com", new List<int> { GroupId });
			UserId = userModel.ArtifactId;
		}

		protected void InstallIntegrationPoints()
		{
			try
			{
				ICoreContext coreContext = SourceProviderTemplate.GetBaseServiceContext(-1);

				var ipAppManager = new RelativityApplicationManager(coreContext, Helper);
				bool isAppInstalled = ipAppManager.IsGetApplicationInstalled(WorkspaceId);
				if (!isAppInstalled)
				{
					ipAppManager.InstallIntegrationPointFromAppLibraryToWorkspace(WorkspaceId);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($@"Unexpected error when detecting or installing Integration Points application in the workspace, Exception: {ex.Message}");
				throw;
			}
			Console.WriteLine("Application is installed.");
		}

		protected void ImportDocuments()
		{
			Console.WriteLine(@"Importing documents...");
			string testDir = TestContext.CurrentContext.TestDirectory.Replace("kCura.IntegrationPoints.UITests",
				"kCura.IntegrationPoint.Tests.Core");
			DocumentsTestData data = DocumentTestDataBuilder.BuildTestData(testDir);
			var workspaceService = new WorkspaceService(new ImportHelper());
			workspaceService.ImportData(WorkspaceId, data);
			Console.WriteLine(@"Documents imported.");
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
			if (!TestContext.CurrentContext.Result.Outcome.Equals(ResultState.Success))
			{
				SaveScreenshot();
			}

			if (WorkspaceId != int.MinValue)
			{
				Workspace.DeleteWorkspace(WorkspaceId);
			}

			Group.DeleteGroup(GroupId);

			User.DeleteUser(UserId);

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
			string testDir = TestContext.CurrentContext.TestDirectory;
			string testName = TestContext.CurrentContext.Test.FullName;
			string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff");
			screenshot.SaveAsFile($@"{testDir}\{timeStamp}_{testName}.png", ScreenshotImageFormat.Png);
		}
	}
}
