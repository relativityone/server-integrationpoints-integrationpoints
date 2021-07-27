using Atata;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Web;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.RingSetup;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Steps;

namespace Relativity.IntegrationPoints.Tests.Functional.CD
{
	[TestExecutionCategory.CD, TestLevel.L3]
	[Feature.DataTransfer.IntegrationPoints]
	public abstract class TestsBase : TestSetup, ITestsImplementationTestFixture
	{
		public Workspace Workspace => _workspace;

		protected TestsBase(string name)
			: base(name, desiredNumberOfDocuments: 0, importImages: false)
		{
		}

		[OneTimeSetUp]
		public virtual void OneTimeSetUp()
		{
			RelativityFacade.Instance.RelyOn<WebComponent>();

			RelativityFacade.Instance.RequireAgent(Const.INTEGRATION_POINTS_AGENT_TYPE_NAME, Const.INTEGRATION_POINTS_AGENT_RUN_INTERVAL);

			InstallIntegrationPoints();

			SetUpAtataContext();

			LoginAsStandardAccount();
		}

		[OneTimeTearDown]
		public virtual void OneTimeTearDown()
		{
			AtataContext.Current?.Dispose();

			CleanUpTestUser();
		}

		private void InstallIntegrationPoints()
		{
			var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

			var app = applicationService.Get(Const.INTEGRATION_POINTS_APPLICATION_NAME);

			if(!applicationService.IsInstalledInWorkspace(Workspace.ArtifactID, app.ArtifactID))
			{
				applicationService.InstallToWorkspace(Workspace.ArtifactID, app.ArtifactID);
			}
		}

		private void SetUpAtataContext(string contextName = null)
		{
			SwitchContext.CurrentFrame = null;

			AtataContextBuilder contextBuilder = AtataContext.Configure();

			if (contextName != null)
				contextBuilder.UseTestName(contextName);

			contextBuilder.Build();
		}

		private void LoginAsStandardAccount(bool useCookies = false)
		{
			LoginSteps.Login(_user.Email, _user.Password, useCookies);
		}

		private void CleanUpTestUser()
		{
			IUserService userService = RelativityFacade.Instance.Resolve<IUserService>();

			int userId = userService.GetByEmail(_user.Email).ArtifactID;

			userService.Delete(userId);
		}
	}
}
