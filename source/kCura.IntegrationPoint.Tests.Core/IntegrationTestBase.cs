
namespace kCura.IntegrationPoint.Tests.Core
{
	using Relativity.Client;

	public abstract class IntegrationTestBase
	{
		protected IntegrationTestBase()
		{
			Helper = new Helper();
			SharedVariables = new SharedVariables();
		}

		public Helper Helper { get; }
		public SharedVariables SharedVariables { get; }

		public IRSAPIClient RsapiClient { get { return Helper.Rsapi.CreateRsapiClient(); } }

	}
}