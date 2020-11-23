#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using Castle.MicroKernel.Registration;
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
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using Relativity.Testing.Identification;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Tests
{
	using Data;
	using Validation;

	[TestType.UI]
	public abstract class UiTest
	{
		private RemoteWebDriver _driver;

		private readonly bool _shouldLoginToRelativity;
		private readonly bool _shouldImportDocuments;

		private readonly Lazy<ITestHelper> _testHelperLazy;

		protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(UiTest));

		protected IWindsorContainer Container { get; }

		public static string Now => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

		public ITestHelper Helper => _testHelperLazy.Value;

		protected TestConfiguration Configuration { get; set; }

		protected TestContext SourceContext { get; set; }
		
		protected TestContext DestinationContext { get; set; }

		/// <summary>
		/// Value is assigned during SetUp phase, before each test is executed.
		/// Property should not be accessed before SetUp phase of test.
		/// </summary>
		protected RemoteWebDriver Driver
		{
			get
			{
				if (_driver == null)
				{
					throw new TestException("Driver should not be accessed before SetUp phase.");
				}
				return _driver;
			}
			private set
			{
				_driver = value;
			}
		}

		protected UiTest(bool shouldLoginToRelativity = true, bool shouldImportDocuments = true)
		{
			_shouldLoginToRelativity = shouldLoginToRelativity;
			_shouldImportDocuments = shouldImportDocuments;

			Container = new WindsorContainer();
			_testHelperLazy = new Lazy<ITestHelper>(() => new TestHelper());
		}

		[OneTimeSetUp]
		protected Task SetupSuiteAsync()
		{
			LogTestStart();
			// enable TLS 1.2 for R1 regression environments
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			Configuration = new TestConfiguration()
				.MergeCustomConfigWithAppSettings()
				.SetupConfiguration()
				.LogConfiguration();

			SourceContext = new TestContext();
			SourceContext.InitUser();
			Task workspaceSetupTask = SetupWorkspaceAsync();
			DestinationContext = new TestContext();
			Task destinationContextWorkspaceTask = DestinationContext.CreateTestWorkspaceAsync(" - dest");

			return Task.WhenAll(workspaceSetupTask, destinationContextWorkspaceTask);
		}

		[SetUp]
		public void SetupDriver()
		{
			Driver = DriverFactory.Create();
			if (_shouldLoginToRelativity)
			{
				EnsureGeneralPageIsOpened();
			}
		}

		private async Task SetupWorkspaceAsync()
		{
			Log.Information("UiTest: SetupWorkspaceAsync - Source");
			if (string.IsNullOrEmpty(SharedVariables.UiUseThisExistingWorkspace))
			{
				await CreateWorkspaceAsync().ConfigureAwait(false);
			}
			else
			{
				Log.Information("Going to use existing workspace '{WorkspaceName}'.", SharedVariables.UiUseThisExistingWorkspace);
				using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
				{
					Relativity.Client.DTOs.Workspace workspace =
						Workspace.FindWorkspaceByName(proxy, SharedVariables.UiUseThisExistingWorkspace);
					SourceContext.WorkspaceId = workspace.ArtifactID;
				}
				SourceContext.WorkspaceName = SharedVariables.UiUseThisExistingWorkspace;
				Log.Information("ID of workspace '{WorkspaceName}': {WorkspaceId}.", SourceContext.WorkspaceName, SourceContext.WorkspaceId);
			}

			bool isIntegrationPointInstalled = await SourceContext.IsIntegrationPointsInstalledAsync().ConfigureAwait(false);
			if (!isIntegrationPointInstalled)
			{
				throw new TestException($"Integration Points should be installed in workspace '{SourceContext.WorkspaceName}'.");
			}

			if (_shouldImportDocuments)
			{
				await ImportDocumentsAsync().ConfigureAwait(false);
			}
		}

		protected virtual Task CreateWorkspaceAsync()
		{
			return SourceContext.CreateTestWorkspaceAsync();
		}

		protected virtual Task ImportDocumentsAsync()
		{
			return SourceContext.ImportDocumentsAsync();
		}

		[TearDown]
		protected void TearDown()
		{
			LogTestStatus();
			LogBrowserLogsIfTestFailed();
			SaveScreenshotIfTestFailed();

			CloseDriver();
		}

		[OneTimeTearDown]
		protected void OneTimeTearDown()
		{
			if(!HasTestFailed())
			{
				DeleteWorkspace(SourceContext);
				DeleteWorkspace(DestinationContext);
			}
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

		private static void LogTestStart()
		{
			NUnit.Framework.TestContext tc = NUnit.Framework.TestContext.CurrentContext;
			Log.Information("Test {TestName} started.", tc.Test.FullName);
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

		private void CloseDriver()
		{
			try
			{
				Driver?.Quit();
				Driver = null;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Quiting Driver failed.");
			}
		}

		protected void DeleteWorkspace(TestContext workspaceContext)
		{
			try
			{
				if (string.IsNullOrEmpty(SharedVariables.UiUseThisExistingWorkspace) && workspaceContext.WorkspaceId != null)
				{
					Workspace.DeleteWorkspace(workspaceContext.GetWorkspaceId());
					Log.Information("Delete workspace: {workspaceId}", workspaceContext.WorkspaceId);
					workspaceContext.WorkspaceId = null;
				}
				workspaceContext.TearDown();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error during deleting workspace: {workspaceName}", workspaceContext.WorkspaceName);
			}
		}

		private static bool HasTestFailed()
		{
			return !NUnit.Framework.TestContext.CurrentContext.Result.Outcome.Equals(ResultState.Success);
		}

		private void EnsureGeneralPageIsOpened()
		{
			var loginPage = new LoginPage(Driver);

			if (loginPage.IsOnLoginPage())
			{
				loginPage.Login(SourceContext.User.Email, SourceContext.User.Password);
			}
			else
			{
				new GeneralPage(Driver).PassWelcomeScreen();
			}
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
					.ImplementedBy<WorkspaceDBContext>()
					.UsingFactoryMethod(k => new WorkspaceDBContext(k.Resolve<IHelper>().GetDBContext(workspaceArtifactId)))
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
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
