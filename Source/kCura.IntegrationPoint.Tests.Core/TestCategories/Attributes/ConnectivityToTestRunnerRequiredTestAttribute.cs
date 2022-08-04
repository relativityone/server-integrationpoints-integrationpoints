using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
    public class ConnectivityToTestRunnerRequiredTestAttribute: CategoryAttribute
    {
        public ConnectivityToTestRunnerRequiredTestAttribute() : base(TestCategories.CONNECTIVITY_TO_TEST_RUNNER_REQUIRED_TEST)
        {
        }
    }
}
