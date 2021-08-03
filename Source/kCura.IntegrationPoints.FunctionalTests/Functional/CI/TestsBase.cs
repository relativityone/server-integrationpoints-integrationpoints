using System.Net.Http;
using Polly;
using Polly.Retry;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Web;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[TestExecutionCategory.CI, TestLevel.L3]
	[Feature.DataTransfer.IntegrationPoints]
	public abstract class TestsBase : UITestFixture, ITestsImplementationTestFixture
	{
		private readonly string _workspaceName;

		public Workspace Workspace { get; private set; }

		protected TestsBase(string workspaceName)
		{
			_workspaceName = workspaceName;
		}

		protected override void OnSetUpFixture()
		{
			base.OnSetUpFixture();

			Workspace = RelativityFacade.Instance.CreateWorkspace(_workspaceName, TestsSetUpFixture.WORKSPACE_TEMPLATE_NAME);

			RelativityFacade.Instance.RequireAgent(Const.INTEGRATION_POINTS_AGENT_TYPE_NAME, Const.INTEGRATION_POINTS_AGENT_RUN_INTERVAL);
		}

		protected override void OnTearDownFixture()
		{
			base.OnTearDownFixture();

			RelativityFacade.Instance.DeleteWorkspace(Workspace);
		}

		public void LoginAsStandardUser()
		{
			RetryPolicy loginAsStandardAccountPolicy = Policy
				.Handle<HttpRequestException>(ex => ex.Message.Contains("The entered E-Mail Address is already associated with another user in the system."))
				.Retry(3);

			loginAsStandardAccountPolicy.Execute(() => LoginAsStandardAccount());
		}
	}
}
