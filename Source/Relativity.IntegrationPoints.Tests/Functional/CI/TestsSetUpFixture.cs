using Atata;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[SetUpFixture]
	public class TestsSetUpFixture
	{
		private const string FUNCTIONAL_STANDARD_ACCOUNT_EMAIL_FORMAT = "rip_func_user{0}@mail.com";

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			RelativityFacade.Instance.RelyOn<CoreComponent>();
			RelativityFacade.Instance.RelyOn<ApiComponent>();
			RelativityFacade.Instance.RelyOn<WebComponent>();

			RelativityFacade.Instance.Resolve<IAccountPoolService>().StandardAccountEmailFormat = FUNCTIONAL_STANDARD_ACCOUNT_EMAIL_FORMAT;
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			AtataContext.Current?.Dispose();
		}
	}
}
