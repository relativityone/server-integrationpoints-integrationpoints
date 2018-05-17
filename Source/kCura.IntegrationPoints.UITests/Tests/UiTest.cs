using System;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Net;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
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
		public static string Now => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

		protected IWindsorContainer Container;
		protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(UiTest));
		protected IConfigurationStore ConfigurationStore;

		public ITestHelper Helper => _help.Value;
		private readonly Lazy<ITestHelper> _help;

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

		public UiTest()
		{
			Container = new WindsorContainer();
			ConfigurationStore = new DefaultConfigurationStore();
			_help = new Lazy<ITestHelper>(() => new TestHelper());
		}
		protected virtual bool InstallLegalHoldApp => false;

		protected virtual void ContextSetUp()
		{
		}

		[OneTimeSetUp]
		protected void SetupSuite()
		{
			Container = new WindsorContainer();
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
			if (string.IsNullOrEmpty(SharedVariables.UiUseThisExistingWorkspace))
			{
				await CreateWorkspaceAsync();

				Task installIntegrationPointsTask = Context.InstallIntegrationPointsAsync();

				await ImportDocumentsAsync();

				ContextSetUp(); // TODO del such things

				await installIntegrationPointsTask;

				if (InstallLegalHoldApp)
				{
					Task installLegalHoldTask = Context.InstallLegalHoldAsync();
					await installLegalHoldTask;
				}
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

			if (string.IsNullOrEmpty(SharedVariables.UiUseThisExistingWorkspace))
			{
				Workspace.DeleteWorkspace(Context.GetWorkspaceId());
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
			Screenshot screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
			string testDir = NUnit.Framework.TestContext.CurrentContext.TestDirectory;
			string className = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
			string methodName = NUnit.Framework.TestContext.CurrentContext.Test.MethodName;
			string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff");
			screenshot.SaveAsFile($@"{testDir}\{timeStamp}_{className}.{methodName}.png", ScreenshotImageFormat.Png);
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
