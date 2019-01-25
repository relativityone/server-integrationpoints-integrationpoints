﻿using System;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium;
using System.Net;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Driver;
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
		private readonly Lazy<ITestHelper> _help;
		
		protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(UiTest));

		protected static readonly List<Tuple<string, string>> DefaultFieldsMapping = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Control Number", "Control Number"),
			new Tuple<string, string>("Extracted Text", "Extracted Text"),
			new Tuple<string, string>("Title", "Title")
		};

		protected IWindsorContainer Container { get; private set; }

		public static string Now => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

		public ITestHelper Helper => _help.Value;

		protected TestConfiguration Configuration { get; set; }

		protected TestContext Context { get; set; }

		/// <summary>
		/// Value is assigned before during SetUp phase, before each test is executed.
		/// Property should not be accessed before SetUp phase of test.
		/// </summary>
		protected RemoteWebDriver Driver { get; set; }

		protected UiTest()
		{
			Container = new WindsorContainer();
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
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			Configuration = new TestConfiguration()
				.MergeCustomConfigWithAppSettings()
				.SetupConfiguration()
				.LogConfiguration();

			Context = new TestContext();
			Context.InitUser();
			Task agentSetupTask = SetupAgentAsync();
			Task workspaceSetupTask = SetupWorkspaceAsync();

			Task.WaitAll(agentSetupTask, workspaceSetupTask);
		}

		[SetUp]
		public void SetupDriver()
		{
			Driver = DriverFactory.CreateDriver();
			EnsureGeneralPageIsOpened();
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

			ContextSetUp(); // TODO del such things

			await installIntegrationPointsTask;

			if (InstallLegalHoldApp)
			{
				Task installLegalHoldTask = Context.InstallLegalHoldAsync();
				await installLegalHoldTask;
			}
		}

		protected virtual async Task CreateWorkspaceAsync()
		{
			await Context.CreateTestWorkspaceAsync();
		}

		protected virtual async Task ImportDocumentsAsync()
		{
			await Context.ImportDocumentsAsync();
		}

		[TearDown]
		protected void CloseAndQuitDriver()
		{
			try
			{
				if (!NUnit.Framework.TestContext.CurrentContext.Result.Outcome.Equals(ResultState.Success))
				{
					SaveScreenshot();
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error during saving screenshot.");
			}

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


		[OneTimeTearDown]
		protected void OneTimeTearDown()
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
		}

		private void EnsureGeneralPageIsOpened()
		{
			var loginPage = new LoginPage(Driver);

			if (loginPage.IsOnLoginPage())
			{
				loginPage.Login(Context.User.Email, Context.User.Password);
			}
			else
			{
				new GeneralPage(Driver).PassWelcomeScreen();
			}
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
