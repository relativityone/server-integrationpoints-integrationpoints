
namespace kCura.IntegrationPoint.Tests.Core
{
	using Relativity.Client;

	public abstract class IntegrationTestBase
	{
		protected IntegrationTestBase()
		{
			Helper = new Helper();
		}

		public Helper Helper { get; }

		public IRSAPIClient RsapiClient { get { return Helper.Rsapi.CreateRsapiClient(); } }

	}
}