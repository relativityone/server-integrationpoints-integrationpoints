using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler;
using ObjectManagerStub = Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler.ObjectManagerStub;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class ProxyMock
	{
		public ObjectManagerStub ObjectManager { get; }

		public WorkspaceManagerStub WorkspaceManager { get; }


		public ProxyMock(InMemoryDatabase database)
		{
			ObjectManager = new ObjectManagerStub(database);
			WorkspaceManager = new WorkspaceManagerStub(database);
		}

		public void Clear()
		{
			ObjectManager.Mock.Reset();
			WorkspaceManager.Mock.Reset();
		}
	}
}
