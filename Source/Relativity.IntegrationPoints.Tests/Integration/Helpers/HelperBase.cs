using Relativity.IntegrationPoints.Tests.Integration.Mocks;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public abstract class HelperBase
	{
		protected readonly InMemoryDatabase Database;

		protected readonly ProxyMock ProxyMock;

		protected readonly HelperManager HelperManager;

		protected HelperBase(HelperManager helperManager, InMemoryDatabase database, ProxyMock proxyMock)
		{
			HelperManager = helperManager;
			Database = database;
			ProxyMock = proxyMock;
		}
	}
}
