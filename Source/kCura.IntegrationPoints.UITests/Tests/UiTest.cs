using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using IntegrationPointsUITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using TestHelper = kCura.IntegrationPoint.Tests.Core.TestHelpers.TestHelper;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Services.Permission;
using Permission = kCura.IntegrationPoint.Tests.Core.Permission;

namespace IntegrationPointsUITests.Tests
{
	public abstract class UiTest
	{
	    protected static int WorkspaceId { get; set; } = int.MinValue;
		protected static string WorkspaceName { get; set; }
        
        protected IWebDriver Driver { get; set; }

		[OneTimeSetUp]
		protected void CreateDriver()
		{
		    kCura.Data.RowDataGateway.Config.MockConfigurationValue("LongRunningQueryTimeout", 100);
            string connString = string.Format(ConfigurationManager.AppSettings["connectionStringEDDS"], "il1ddmlpl3db001.kcura.corp", "EDDSdbo", "P@ssw0rd@1");
            kCura.Config.Config.SetConnectionString(connString);
		    global::Relativity.Data.Config.InjectConfigSettings(new Dictionary<string, object>
		    {
		        {"connectionString", SharedVariables.EddsConnectionString}
		    });
            

            string testTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
		    WorkspaceName = $"Test Workspace {testTimeStamp}";

            try
		    {
		        WorkspaceId = Workspace.CreateWorkspace($"Test Workspace {testTimeStamp}", "kCura Starter Template");
		    }
		    catch (NullReferenceException ex)
		    {
                Console.WriteLine($@"Cannot create workspace. Check if Relativity works correctly (services, ...). Exception: {ex}.");
                throw;
		    }

		    // setup user and group
			// create group for 
			int groupId = Group.CreateGroup($"TestGroup_{testTimeStamp}");
			Group.AddGroupToWorkspace(WorkspaceId, groupId);
		    GroupPermissions permissions = Permission.GetGroupPermissions(WorkspaceId, groupId);




		    ClaimsPrincipal.ClaimsPrincipalSelector += () =>
		    {
		        var factory = new ClaimsPrincipalFactory();
		        var _ADMIN_USER_ID = 9;
		        return factory.CreateClaimsPrincipal2(_ADMIN_USER_ID);
		    };

            try
		    {
		        ObjectPermission permissionsForRdo =
		            permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointType);
		        permissionsForRdo.ViewSelected = false;
		    }
		    catch (Exception) // probably no IP in workspace
		    {
		        ICoreContext coreContext = GetBaseServiceContext(ClaimsPrincipal.Current, -1);
		        var ipAppManager = new RelativityApplicationManager(coreContext, new TestHelper());
		        ipAppManager.InstallIntegrationPointFromAppLibraryToWorkspace(WorkspaceId);

		        ObjectPermission permissionsForRdo =
		            permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointType);
		        permissionsForRdo.ViewSelected = false;
            }


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
			Driver.Url = SharedVariables.TargetHost + "/Relativity";
			Driver.Url = "https://il1ddmlpl3wb001.kcura.corp/Relativity/";

		}

	    private ICoreContext GetBaseServiceContext(ClaimsPrincipal claimsPrincipal, int workspaceId)
	    {
	        try
	        {
	            return claimsPrincipal.GetServiceContextUnversionShortTerm(workspaceId);
	        }
	        catch (Exception exception)
	        {
	            throw new Exception("Unable to initialize the user context.", exception);
	        }
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
