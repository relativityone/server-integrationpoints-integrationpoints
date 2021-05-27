using System.Linq;
using System.Net.Http;
using Polly;
using Polly.Retry;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Web;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[TestExecutionCategory.CI, TestLevel.L3]
	[Feature.DataTransfer.IntegrationPoints]
	public abstract class TestsBase : UITestFixture
	{
		private const string INTEGRATION_POINTS_AGENT_TYPE_NAME = "Integration Points Agent";
		private const int INTEGRATION_POINTS_AGENT_RUN_INTERVAL = 5;

		private readonly string _workspaceName;
		protected Workspace _workspace;

		protected TestsBase(string workspaceName)
		{
			_workspaceName = workspaceName;
		}

		protected override void OnSetUpFixture()
		{
			base.OnSetUpFixture();

			_workspace = RelativityFacade.Instance.CreateWorkspace(_workspaceName, TestsSetUpFixture.WORKSPACE_TEMPLATE_NAME);

			IAgentService agentService = RelativityFacade.Instance.Resolve<IAgentService>();
			agentService.Require(new Agent
			{
				AgentType = agentService.GetAgentType(INTEGRATION_POINTS_AGENT_TYPE_NAME),
				AgentServer = agentService.GetAvailableAgentServers(INTEGRATION_POINTS_AGENT_TYPE_NAME).First(),
				RunInterval = INTEGRATION_POINTS_AGENT_RUN_INTERVAL
			});
		}

		protected override void OnTearDownFixture()
		{
			base.OnTearDownFixture();

			RelativityFacade.Instance.DeleteWorkspace(_workspace);
		}

		protected override void OnSetUpTest()
		{
			base.OnSetUpTest();

			RetryPolicy loginAsStandardAccountPolicy = Policy
				.Handle<HttpRequestException>(ex => ex.Message.Contains("The entered E-Mail Address is already associated with another user in the system."))
				.Retry(3);

			loginAsStandardAccountPolicy.Execute(() => LoginAsStandardAccount());
		}
	}
}
