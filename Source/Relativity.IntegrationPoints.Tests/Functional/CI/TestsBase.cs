using System.Net.Http;
using Polly;
using Polly.Retry;
using Relativity.Testing.Framework.Web;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[TestExecutionCategory.CI, TestLevel.L3]
	[Feature.DataTransfer.IntegrationPoints]
	public abstract class TestsBase : UITestFixture
	{
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
