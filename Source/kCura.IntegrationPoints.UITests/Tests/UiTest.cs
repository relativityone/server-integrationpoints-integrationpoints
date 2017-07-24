using System;
using System.Collections.Generic;
using IntegrationPointsUITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace IntegrationPointsUITests.Tests
{
	public abstract class UiTest
	{
		private static int _workspaceId;
		protected IWebDriver Driver { get; set; }

		[OneTimeSetUp]
		protected void CreateDriver()
		{
			var testTimeStamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

			// test workspace setup using API
			_workspaceId = Workspace.CreateWorkspace($"TestWorkspace{testTimeStamp}");

			// setup user and group
			// create group for 
			int groupId = Group.CreateGroup($"TestGroup_{testTimeStamp}");
			Group.AddGroupToWorkspace(_workspaceId, groupId);
			var permissions = Permission.GetGroupPermissions(_workspaceId, groupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointType);
			permissionsForRDO.ViewSelected = false;

			User.CreateUser("John", $"Doe_{testTimeStamp}", $"test_{testTimeStamp}@kcura.com", new List<int> { groupId });



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
			//Driver.Url = SharedVariables.TargetHost;
			Driver.Url = "https://il1ddmlpl3wb001.kcura.corp/Relativity/";

		}

		[OneTimeTearDown]
		protected void CloseAndQuitDriver()
		{
			if (!TestContext.CurrentContext.Result.Outcome.Equals(ResultState.Success))
			{
				SaveScreenshot();
			}
			Workspace.DeleteWorkspace(_workspaceId);

			Driver.Quit();
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
			Screenshot screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
			string testDir = TestContext.CurrentContext.TestDirectory;
			string testName = TestContext.CurrentContext.Test.FullName;
			string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff");
			screenshot.SaveAsFile($@"{testDir}\{timeStamp}_{testName}.png", ScreenshotImageFormat.Png);
		}
	}
}
