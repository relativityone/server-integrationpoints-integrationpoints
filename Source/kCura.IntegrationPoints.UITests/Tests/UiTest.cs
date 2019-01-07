using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.Relativity.Client;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using Relativity.API;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Tests
{
	using Data;
	using Validation;

	public abstract class UiTest
	{
		
		private readonly Lazy<ITestHelper> _help;

		protected IConfigurationStore ConfigurationStore;

		protected IWindsorContainer Container;

		protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(UiTest));
		
		public static string Now => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

		public ITestHelper Helper => _help.Value;

		protected TestConfiguration Configuration { get; set; }

		protected TestContext Context { get; set; }

		protected RemoteWebDriver Driver { get; set; }

		protected UiTest()
		{
			Container = new WindsorContainer();
			ConfigurationStore = new DefaultConfigurationStore();
			_help = new Lazy<ITestHelper>(() => new TestHelper());
		}
		
		[OneTimeSetUp]
		protected void SetupSuite()
		{
			Container = new WindsorContainer();
			// enable TLS 1.2 for R1 regression environments
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			Configuration = new TestConfiguration()
				.MergeCustomConfigWithAppSettings()
				.SetupConfiguration()
				.LogConfiguration();

			Context = new TestContext();
			Context.InitUser();
			Task agentSetupTask = SetupAgentAsync();
			Task workspaceSetupTask = SetupWorkspaceAsync();
			Task webDriverCreationTask = CreateDriverAsync();

			Task.WaitAll(agentSetupTask, workspaceSetupTask, webDriverCreationTask);
		}

		private async Task SetupAgentAsync()
		{
			await Task.Run(() => Agent.CreateIntegrationPointAgentIfNotExists());
		}

		private async Task SetupWorkspaceAsync()
		{
			if (string.IsNullOrEmpty(SharedVariables.UiUseThisExistingWorkspace))
			{
				await CreateWorkspaceAsync();
			}
			else
			{
				Log.Information("Going to use existing workspace '{WorkspaceName}'.", SharedVariables.UiUseThisExistingWorkspace);
				using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
				{
					Relativity.Client.DTOs.Workspace workspace =
						Workspace.FindWorkspaceByName(proxy, SharedVariables.UiUseThisExistingWorkspace);
					Context.WorkspaceId = workspace.ArtifactID;
				}
				Context.WorkspaceName = SharedVariables.UiUseThisExistingWorkspace;
				Log.Information("ID of workspace '{WorkspaceName}': {WorkspaceId}.", Context.WorkspaceName, Context.WorkspaceId);
			}

			Task installIntegrationPointsTask = Context.InstallIntegrationPointsAsync();

			if (!SharedVariables.UiSkipDocumentImport)
			{
				await ImportDocumentsAsync();
			}

			await installIntegrationPointsTask;
		}

		protected virtual async Task CreateWorkspaceAsync()
		{
			await Context.CreateTestWorkspaceAsync();
		}

		protected virtual async Task ImportDocumentsAsync()
		{
			await Context.ImportDocumentsAsync();
		}

		protected async Task CreateDriverAsync()
		{
			await Task.Run(() => Driver = DriverFactory.Create()).ConfigureAwait(false);
		}

		[TearDown]
		protected void LogTestErrors()
		{
			LogTestStatus();

			LogBrowserLogsIfTestFailed();
		}

		private void LogBrowserLogsIfTestFailed()
		{
			try
			{
				if (HasTestFailed())
				{
					ReadOnlyCollection<LogEntry> entries = Driver.Manage().Logs.GetLog(LogType.Browser);
					var builder = new StringBuilder();
					foreach (LogEntry entry in entries)
					{
						builder.AppendLine(entry.ToString());
					}

					Log.Error("Browser logs:\n{BrowserLogs}", builder.ToString());
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error during accessing browser logs.");
			}
		}

		private static void LogTestStatus()
		{
			try
			{
				NUnit.Framework.TestContext tc = NUnit.Framework.TestContext.CurrentContext;
				if (HasTestFailed())
				{
					Log.Error("Test {TestName} finished unsuccessfully. Status: {TestStatus},\n" +
					          "message: '{ErrorMessage}', stacktrace:\n{TestStacktrace}",
						tc.Test.FullName, tc.Result.Outcome.Status, tc.Result.Message, tc.Result.StackTrace);
				}
				else
				{
					Log.Information("Test {TestName} finished successfully.", tc.Test.FullName);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error during logging test status.");
			}
		}

		[TearDown]
		private void SaveScreenshotIfTestFailed()
		{
			try
			{
				if (HasTestFailed())
				{
					ScreenshotSaver.SaveScreenshot(Driver);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error during saving screenshot.");
			}
		}

		private static bool HasTestFailed()
		{
			return !NUnit.Framework.TestContext.CurrentContext.Result.Outcome.Equals(ResultState.Success);
		}

		[OneTimeTearDown]
		protected void CloseAndQuitDriver()
		{
			try
			{
				if (string.IsNullOrEmpty(SharedVariables.UiUseThisExistingWorkspace) && Context.WorkspaceId != null)
				{
					Workspace.DeleteWorkspace(Context.GetWorkspaceId());
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error during deleting workspace.");
			}

			try
			{
				Context.TearDown();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error in Context TearDown.");
			}

			try
			{
				Driver?.Quit();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Quiting Driver failed.");
			}
		}

		protected GeneralPage EnsureGeneralPageIsOpened()
		{
			var loginPage = new LoginPage(Driver);
			if (loginPage.IsOnLoginPage())
			{
				return loginPage.Login(Context.User.Email, Context.User.Password);
			}
			return new GeneralPage(Driver).PassWelcomeScreen();
		}
		
		protected void WaitForJobToFinishAndValidateCompletedStatus(IntegrationPointDetailsPage detailsPage)
		{
			new BaseUiValidator().ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}

		protected void Install(int workspaceArtifactId)
		{
			Container.Register(Component.For<IHelper>().UsingFactoryMethod(k => Helper, managedExternally: true));
			Container.Register(Component.For<IRsapiClientWithWorkspaceFactory>().ImplementedBy<RsapiClientWithWorkspaceFactory>().LifestyleTransient());
			Container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IHelper helper = k.Resolve<IHelper>();
					return new TestServiceContextHelper(helper, workspaceArtifactId);
				}));
			Container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceContext>()
					.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(workspaceArtifactId)))
					.LifeStyle.Transient);
			Container.Register(
				Component.For<IRSAPIClient>()
					.UsingFactoryMethod(k =>
					{
						IRSAPIClient client = Rsapi.CreateRsapiClient();
						client.APIOptions.WorkspaceID = workspaceArtifactId;
						return client;
					})
					.LifeStyle.Transient);

			Container.Register(Component.For<IRSAPIService>().Instance(new RSAPIService(Container.Resolve<IHelper>(), workspaceArtifactId)).LifestyleTransient());
		}
	}
}
