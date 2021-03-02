using Moq;
using ObjectManagerStub = Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler.ObjectManagerStub;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class ProxyMock
	{
		public ObjectManagerStub ObjectManager { get; }


		public ProxyMock(InMemoryDatabase database)
		{
			ObjectManager = new ObjectManagerStub(database);
		}

		public void Clear()
		{
			ObjectManager.Mock.Reset();
		}
	}
}
