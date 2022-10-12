using System.Net.Http;
using Polly;
using Polly.Retry;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestExecutionCategory.CI]
    [TestLevel.L3]
    [Feature.DataTransfer.IntegrationPoints]
    public abstract class TestsBase : UITestFixture, ITestsImplementationTestFixture
    {
        private readonly string _workspaceName;

        private readonly int _existingWorkspaceArtifactID = TestConfig.ExistingWorkspaceArtifactId;

        protected TestsBase(string workspaceName)
        {
            _workspaceName = workspaceName;
        }

        public Workspace Workspace { get; private set; }

        public void LoginAsStandardUser()
        {
            RetryPolicy loginAsStandardAccountPolicy = Policy
                .Handle<HttpRequestException>(ex => ex.Message.Contains("The entered E-Mail Address is already associated with another user in the system."))
                .Retry(3);

            loginAsStandardAccountPolicy.Execute(() => LoginAsStandardAccount());
        }

        protected override void OnSetUpFixture()
        {
            base.OnSetUpFixture();

            Workspace = _existingWorkspaceArtifactID != 0
                ? RelativityFacade.Instance.GetExistingWorkspace(_existingWorkspaceArtifactID)
                : RelativityFacade.Instance.CreateWorkspace(_workspaceName, TestsSetUpFixture._WORKSPACE_TEMPLATE_NAME);

            RelativityFacade.Instance.RequireAgent(Const.INTEGRATION_POINTS_AGENT_TYPE_NAME, Const.INTEGRATION_POINTS_AGENT_RUN_INTERVAL);
        }

        protected override void OnTearDownFixture()
        {
            base.OnTearDownFixture();
            if (_existingWorkspaceArtifactID == 0)
            {
                RelativityFacade.Instance.DeleteWorkspace(Workspace);
            }
        }
    }
}
