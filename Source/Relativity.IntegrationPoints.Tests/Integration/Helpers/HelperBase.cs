using Relativity.IntegrationPoints.Tests.Integration.Mocks;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public abstract class HelperBase
	{
		protected readonly InMemoryDatabase Database;

		protected readonly ProxyMock ProxyMock;

		protected HelperBase(InMemoryDatabase database, ProxyMock proxyMock)
		{
			Database = database;
			ProxyMock = proxyMock;
		}
	}
}
